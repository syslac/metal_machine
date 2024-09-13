using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Java.Util.Logging;
using MetalMachine.Models;
using MetalMachine.Services;

namespace MetalMachine.ViewModels;

public partial class LandingViewModel : BaseViewModel
{
    private double _distance;
    private double _maxDistance;
    private string _maxDistancePlace;
    private string _maxDistanceBand;
    private double _avgDistance;
    private long _numConcerts;
    private long _numDays;
    private long _numEstimatedTrips;
    public LandingViewModel(IDBManager db, IGeocoding g, IPreferences p, IConcertProvider c, IMessenger m) : base(db, g, p, c, m) 
    {
        CsvIsInProgress = false;
        CsvProgress = String.Empty;
        LastUserLocation = new Location(0, 0);

        _messenger.Register<SetlistFmSong>(this, (recipient, song) => RereadDb(song));
    }

    public string Distance => _distance.ToString("n2");
    public bool ShowDistance { get; set; }
    public string MaxDistance => _maxDistance.ToString("n2");
    public bool ShowMaxDistance { get; set; }
    public string AvgDistance => _avgDistance.ToString("n2");
    public bool ShowAvgDistance { get; set; }
    public string NumConcerts => _numConcerts.ToString();
    public bool ShowNumConcerts { get; set; }
    public string NumDays => _numDays.ToString();
    public bool ShowNumDays { get; set; }
    public string NumEstimatedTrips => _numEstimatedTrips.ToString();
    public bool ShowNumTrips { get; set; }
    public string MaxDistancePlace => ((_maxDistancePlace?.IndexOf(',') ?? -1) >= 0) ? _maxDistancePlace?.Substring(0, _maxDistancePlace?.IndexOf(',') ?? 0) ?? "" : _maxDistancePlace;
    public bool ShowMaxDistancePlace { get; set; }
    public string MaxDistanceBand => _maxDistanceBand;
    public bool ShowMaxDistanceBand { get; set; }

    public string CsvProgress { get; set; }
    public bool CsvIsInProgress { get; set; }

    public Location LastUserLocation { get; set; }

    public override async Task OnAppearing() 
    {
        await base.OnAppearing();

        if (LastUserLocation != CurrentLocation) 
        {
            LastUserLocation = CurrentLocation;
            await LoadData();
        }

        OnPropertyChanged(nameof(Distance));
        OnPropertyChanged(nameof(AvgDistance));
        OnPropertyChanged(nameof(MaxDistance));
        OnPropertyChanged(nameof(MaxDistancePlace));
        OnPropertyChanged(nameof(NumConcerts));
        OnPropertyChanged(nameof(NumDays));
        OnPropertyChanged(nameof(NumEstimatedTrips));
        OnPropertyChanged(nameof(MaxDistanceBand));
    }

    internal async void RereadDb(SetlistFmSong song) 
    {
        await LoadData();
        OnPropertyChanged(nameof(Distance));
        OnPropertyChanged(nameof(AvgDistance));
        OnPropertyChanged(nameof(MaxDistance));
        OnPropertyChanged(nameof(MaxDistancePlace));
        OnPropertyChanged(nameof(NumConcerts));
        OnPropertyChanged(nameof(NumDays));
        OnPropertyChanged(nameof(NumEstimatedTrips));
        OnPropertyChanged(nameof(MaxDistanceBand));
    }

    private async Task LoadData() 
    {
        
        CsvIsInProgress = true;
        CsvProgress = "Loading";
        OnPropertyChanged(nameof(CsvIsInProgress));
        OnPropertyChanged(nameof(CsvProgress));

        List<Concert> concertList = (await _dbManager.GetAllConcerts(CurrentUser.Id)) ?? [];
        _numConcerts = concertList?.Count ?? 0;
        _distance = 0;
        _avgDistance = 0;
        _numDays = 0;
        _numEstimatedTrips = 0;
        _maxDistance = 0;
        Concert prevConcert = null;
        Dictionary<string, double> bandCompetition = [];
        foreach (var concert in concertList)
        {
            if (prevConcert is null 
                ||prevConcert?.Date.Year != concert.Date.Year 
                || prevConcert?.Date.Month != concert.Date.Month
                || prevConcert?.Date.Day != concert.Date.Day
                ) 
            {
                _numDays++;
            }
            if (prevConcert is null
                || concert.didTravelBetweenConcerts(prevConcert, CurrentLocation) != false)
            {
                double dist = Location.CalculateDistance(CurrentLocation, concert.Address, DistanceUnits.Kilometers);
                _distance += dist;
                _avgDistance += dist;
                if (dist > _maxDistance)
                {
                    _maxDistance = dist;
                    _maxDistancePlace = concert.AddressName;
                }
                _numEstimatedTrips++;
                if (bandCompetition.TryGetValue(concert.Name, out double currKm)) 
                {
                    bandCompetition[concert.Name] = currKm + dist;
                }
                else 
                {
                    bandCompetition.Add(concert.Name, dist);
                }
            }
            // even if there was no travel from previous concert to this one,
            // we should count it towards bands totals, otherwise this makes
            // no sense (only 1st band of festival gets counted, e.g.)
            else if (prevConcert is not null && concert.didTravelBetweenConcerts(prevConcert, CurrentLocation) == false) 
            {
                double dist = Location.CalculateDistance(CurrentLocation, concert.Address, DistanceUnits.Kilometers);
                if (bandCompetition.TryGetValue(concert.Name, out double currKm)) 
                {
                    bandCompetition[concert.Name] = currKm + dist;
                }
                else 
                {
                    bandCompetition.Add(concert.Name, dist);
                }

            }
            prevConcert = concert;
        }
        _avgDistance /= (double)_numEstimatedTrips;
        (string, double) maxBand = ("", 0);
        foreach (var el in bandCompetition) 
        {
            if (el.Value > maxBand.Item2) 
            {
                maxBand.Item1 = el.Key;
                maxBand.Item2 = el.Value;
            }
        }
        _maxDistanceBand = maxBand.Item1;

        CsvIsInProgress = false;
        CsvProgress = String.Empty;
        OnPropertyChanged(nameof(CsvIsInProgress));
        OnPropertyChanged(nameof(CsvProgress));
    }

    [RelayCommand]
    public void ToggleVisible (byte iconNum) 
    {
        switch (iconNum)
        {
            case 0x01:
                ShowDistance = !ShowDistance;
                OnPropertyChanged(nameof(ShowDistance));
                break;
            case 0x02:
                ShowNumTrips = !ShowNumTrips;
                OnPropertyChanged(nameof(ShowNumTrips));
                break;
            case 0x03:
                ShowNumConcerts = !ShowNumConcerts;
                OnPropertyChanged(nameof(ShowNumConcerts));
                break;
            case 0x04:
                ShowNumDays = !ShowNumDays;
                OnPropertyChanged(nameof(ShowNumDays));
                break;
            case 0x05:
                ShowMaxDistance = !ShowMaxDistance;
                OnPropertyChanged(nameof(ShowMaxDistance));
                break;
            case 0x06:
                ShowMaxDistancePlace = !ShowMaxDistancePlace;
                OnPropertyChanged(nameof(ShowMaxDistancePlace));
                break;
            case 0x07:
                ShowAvgDistance = !ShowAvgDistance;
                OnPropertyChanged(nameof(ShowAvgDistance));
                break;
            case 0x08:
                ShowMaxDistanceBand = !ShowMaxDistanceBand;
                OnPropertyChanged(nameof(ShowMaxDistanceBand));
                break;
            default:
                break;
        }
    }

}
