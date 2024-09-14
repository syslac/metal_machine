using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Android.App;
using Android.Util;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Java.Util.Logging;
using MetalMachine.Models;
using MetalMachine.Services;

namespace MetalMachine.ViewModels;

public partial class MaintenanceViewModel : BaseViewModel
{
    public MaintenanceViewModel(IDBManager db, IGeocoding g, IPreferences p, IConcertProvider c, IMessenger m) : base(db, g, p, c, m) 
    {
        CsvIsInProgress = false;
        CsvProgress = String.Empty;
    }


    public override async Task OnAppearing() 
    {
        await base.OnAppearing();

    }

    [RelayCommand]
    public void ClearCache () 
    {
        _dbManager.ReinitDb();
        _messenger.Send<SetlistFmSong>();
    }

    [RelayCommand]
    public async Task LoadCsv () 
    {
        try 
        {
            var result = await FilePicker.PickAsync(PickOptions.Default);
            if (result is not null) 
            {
                CsvIsInProgress = true;
                CsvProgress = "Loading CSV...";
                OnPropertyChanged(nameof(CsvIsInProgress));
                OnPropertyChanged(nameof(CsvProgress));

                using var stream = await result.OpenReadAsync();
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                string csvText = String.Empty;
                int i = 0;
                while (csvText is not null) 
                {
                    csvText = await reader?.ReadLineAsync();
                    if (csvText is not null) 
                    {
                        string[] chunks = csvText.Split(";");
                        if (chunks.Length >= 4) 
                        {
                            // [0] is the artist
                            // [2] is the address name
                            // [3] is the date, in yyyy-mm-dd
                            // I can pass an empty new Location here, it will
                            // not be used inside the method
                            Concert toAdd = new Concert(chunks[0], new Location(), DateTime.ParseExact(chunks[3], "yyyy-MM-dd", CultureInfo.InvariantCulture), chunks[2]);
                            await _dbManager.AddConcert(CurrentUser.Id, toAdd);

                            // 1s wait to not hammer the Geocoding API
                            await Task.Delay(1000);
                        }
                    }
                    i++;
                    CsvProgress = $"Added {i} concerts";
                    OnPropertyChanged(nameof(CsvProgress));
                }
            }
        }
        catch (Exception e) 
        {
        }
        finally 
        {
            CsvIsInProgress = false;
            CsvProgress = String.Empty;
            OnPropertyChanged(nameof(CsvIsInProgress));
            OnPropertyChanged(nameof(CsvProgress));

            // Send request to main page to recalc
            _messenger.Send<SetlistFmSong>();
        }
    }

    [RelayCommand]
    public async Task RefreshConcertDbSetlistFm () 
    {
        RetrySuggestion result = RetrySuggestion.Good;
        CsvIsInProgress = true;
        CsvProgress = String.Empty;
        OnPropertyChanged(nameof(CsvIsInProgress));
        OnPropertyChanged(nameof(CsvProgress));

        int i = 1;
        try {
            do 
            {
                result = await _concertProvider.PopulateConcertList(_currUser.Name);
                if (result == RetrySuggestion.WaitAndRetry)
                {
                    Log.Debug("Api call suggests to wait: sleeping 2000 ms", result.ToString());
                    await Task.Delay(2000);
                }
                CsvProgress = $"Retrieved page {i} from setlist.fm";
                OnPropertyChanged(nameof(CsvProgress));
                i++;
            } 
            while (result != RetrySuggestion.Stop);
            Log.Info("Api calls ended", result.ToString());

            Concert? concert = null;
            i = 1;
            do 
            {
                concert = _concertProvider.GetNextConcert();
                if (concert is not null)
                {
                    await _dbManager.AddConcert(_currUser.Id, concert);
                }
                CsvProgress = $"Added concert {i} to db";
                OnPropertyChanged(nameof(CsvProgress));
                i++;
            } 
            while (concert is not null);
        }
        catch (Exception ex)
        {
            Log.Warn("Error in getting data from API", ex.Message);
        }
        finally 
        {
            CsvIsInProgress = false;
            CsvProgress = String.Empty;
            OnPropertyChanged(nameof(CsvIsInProgress));
            OnPropertyChanged(nameof(CsvProgress));

            // Send request to main page to recalc
            _messenger.Send<SetlistFmSong>();
        }
    }

    [RelayCommand]
    public async Task DownloadDb () 
    {
        try 
        {
            FileStream dbFile = File.OpenRead(_dbManager.ToString());
            MemoryStream dbRaw = new MemoryStream();
            await dbFile.CopyToAsync(dbRaw);
            //using var streamOut = new MemoryStream(dbRaw.ToArray());
            var fileSaverResult = await FileSaver.SaveAsync($"backup_{DateTime.Now.ToString("s")}.db", dbRaw, null);
            if (!fileSaverResult.IsSuccessful)
            {
                Log.Error("Error saving db backup", fileSaverResult.Exception.Message);
            }
            dbFile.Close();
        }
        catch (Exception e) 
        {
            Log.Error("Error saving db backup", e.Message);
        }
    }

    [RelayCommand]
    public async Task ImportDb () 
    {
        try 
        {
            var result = await FilePicker.PickAsync(PickOptions.Default);
            if (result is not null) 
            {
                CsvIsInProgress = true;
                CsvProgress = "Loading Database...";
                OnPropertyChanged(nameof(CsvIsInProgress));
                OnPropertyChanged(nameof(CsvProgress));

                using var stream = await result.OpenReadAsync();
                StreamReader reader = new StreamReader(stream);
                MemoryStream memoryStream = new MemoryStream();
                await reader.BaseStream.CopyToAsync(memoryStream);

                FileStream dbFile = File.OpenWrite(_dbManager.ToString());
                dbFile.WriteAsync(memoryStream.ToArray());

                dbFile.Close();
            }
        }
        catch (Exception e) 
        {
            Log.Error("Error loading db from backup", e.Message);
        }
        finally 
        {
            CsvIsInProgress = false;
            CsvProgress = String.Empty;
            OnPropertyChanged(nameof(CsvIsInProgress));
            OnPropertyChanged(nameof(CsvProgress));

            // Send request to main page to recalc
            _messenger.Send<SetlistFmSong>();
        }
    }
}

