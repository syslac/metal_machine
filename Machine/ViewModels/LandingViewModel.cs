using System;
using System.Runtime.CompilerServices;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Java.Util.Logging;
using MetalMachine.Models;
using MetalMachine.Services;

namespace MetalMachine.ViewModels;

public partial class LandingViewModel : BaseViewModel
{
    private double _latitude;
    private double _distance;
    private double _maxDistance;
    private string _maxDistancePlace;
    private double _avgDistance;
    private long _numConcerts;
    public LandingViewModel(IDBManager db, IGeocoding g, IPreferences p) : base(db, g, p) 
    {
        CsvIsInProgress = false;
        CsvProgress = String.Empty;
    }

    public string Latitude => String.Format("{0}", _latitude);
    public string Distance => String.Format("{0}", _distance);
    public string MaxDistance => String.Format("{0}", _maxDistance);
    public string AvgDistance => String.Format("{0}", _avgDistance);
    public string NumConcerts => String.Format("{0}", _numConcerts);
    public string MaxDistancePlace => _maxDistancePlace;

    public string CsvProgress { get; set; }
    public bool CsvIsInProgress { get; set; }

    public override async Task OnAppearing() 
    {
        await base.OnAppearing();
        // test code, no longer needed
        //_dbManager.AddAddress("Test", new Location(45.5, 12.5));
        //Location? res = await _dbManager.GetCoordinates("Test");
        //if (res is not null) 
        //{
        //    _latitude = res?.Latitude ?? 0;
        //    OnPropertyChanged(nameof(Latitude));
        //}

        List<Concert> concertList = await _dbManager.GetAllConcerts(CurrentUser.Name);
        _numConcerts = concertList.Count;
        _maxDistance = 0;
        foreach (var concert in concertList)
        {
            double dist = Location.CalculateDistance(CurrentLocation, concert.Address, DistanceUnits.Kilometers);
            _distance += dist;
            _avgDistance += dist/(double)_numConcerts;
            if (dist > _maxDistance)
            {
                _maxDistance = dist;
                _maxDistancePlace = concert.AddressName;
            }
        }
        OnPropertyChanged(nameof(Distance));
        OnPropertyChanged(nameof(AvgDistance));
        OnPropertyChanged(nameof(MaxDistance));
        OnPropertyChanged(nameof(MaxDistancePlace));
        OnPropertyChanged(nameof(NumConcerts));
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
                        if (chunks.Length >= 3) 
                        {
                            // [2] is the "short location"
                            long locationId;
                            (Location?, long?) existingCoordinates = await _dbManager.GetCoordinates(chunks[2]);
                            if (existingCoordinates.Item2 is null)
                            {
                                Location geocoded = (await _geocoding.GetLocationsAsync(chunks[2])).FirstOrDefault();
                                locationId = await _dbManager.AddAddress(chunks[2], geocoded);
                            }
                            else 
                            {
                                locationId = existingCoordinates.Item2 ?? -1;
                            }
                            // [0] is the artist
                            await _dbManager.AddConcert(CurrentUser.Id, chunks[0], locationId);
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

}
