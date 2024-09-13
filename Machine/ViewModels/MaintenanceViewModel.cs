using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Android.App;
using Android.Util;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Java.Util.Logging;
using MetalMachine.Models;
using MetalMachine.Services;

namespace MetalMachine.ViewModels;

public partial class MaintenanceViewModel : BaseViewModel
{
    public MaintenanceViewModel(IDBManager db, IGeocoding g, IPreferences p, IConcertProvider c) : base(db, g, p, c) 
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
        }
    }

    [RelayCommand]
    public async Task RefreshConcertDbSetlistFm () 
    {
        RetrySuggestion result = RetrySuggestion.Good;
        do 
        {
            result = await _concertProvider.PopulateConcertList(_currUser.Name);
            if (result == RetrySuggestion.WaitAndRetry)
            {
                Log.Debug("Api call suggests to wait: sleeping 2000 ms", result.ToString());
                await Task.Delay(2000);
            }
        } 
        while (result != RetrySuggestion.Stop);
        Log.Info("Api calls ended", result.ToString());

        Concert? concert = null;
        do 
        {
            concert = _concertProvider.GetNextConcert();
            if (concert is not null)
            {
                await _dbManager.AddConcert(_currUser.Id, concert);
            }
        } 
        while (concert is not null);
    }

    [RelayCommand]
    public async Task DownloadDb () 
    {
        //using var stream = await FileSystem.AppDataDirectory.
        //ReadLines
        //using var stream = new MemoryStream(Encoding.Default.GetBytes("Hello from the Community Toolkit!"));
        //var fileSaverResult = await FileSaver.Default.SaveAsync("test.db", stream, null);   
    }

}

