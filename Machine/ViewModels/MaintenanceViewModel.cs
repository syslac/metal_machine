using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Java.Util.Logging;
using MetalMachine.Models;
using MetalMachine.Services;

namespace MetalMachine.ViewModels;

public partial class MaintenanceViewModel : BaseViewModel
{
    public MaintenanceViewModel(IDBManager db, IGeocoding g, IPreferences p) : base(db, g, p) 
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
                            // [2] is the "short location"
                            long locationId;
                            (Location?, long?) existingCoordinates = await _dbManager.GetCoordinates(chunks[2]);
                            if (existingCoordinates.Item2 is null)
                            {
                                Location geocoded = (await _geocoding.GetLocationsAsync(chunks[2])).FirstOrDefault();
                                // 1s wait to not hammer the Geocoding API
                                await Task.Delay(1000);
                                locationId = await _dbManager.AddAddress(chunks[2], geocoded);
                            }
                            else 
                            {
                                locationId = existingCoordinates.Item2 ?? -1;
                            }
                            // [0] is the artist
                            // [3] is the date, in yyyy-mm-dd
                            await _dbManager.AddConcert(CurrentUser.Id, chunks[0], locationId, DateTime.ParseExact(chunks[3], "yyyy-MM-dd", CultureInfo.InvariantCulture));
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
    public async Task DownloadDb () 
    {
        //using var stream = await FileSystem.AppDataDirectory.
        //ReadLines
        //using var stream = new MemoryStream(Encoding.Default.GetBytes("Hello from the Community Toolkit!"));
        //var fileSaverResult = await FileSaver.Default.SaveAsync("test.db", stream, null);   
    }

}

