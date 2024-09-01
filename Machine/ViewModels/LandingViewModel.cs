using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MetalMachine.Services;

namespace MetalMachine.ViewModels;

public partial class LandingViewModel : BaseViewModel
{
    private double _latitude;
    private double _distance;
    public LandingViewModel(IDBManager db, IGeocoding g, IPreferences p) : base(db, g, p) 
    {
    }

    public string Latitude => String.Format("{0}", _latitude);
    public string Distance => String.Format("{0}", _distance);

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

        string [] addresses = ["Rome, Italy"];
        Location [] positions = [new Location(), new Location()];
        for (int i = 0; i < addresses.Length; i++) 
        {
            positions[i] = (await _geocoding.GetLocationsAsync(addresses[i])).FirstOrDefault();
        }
        _distance = Location.CalculateDistance(CurrentLocation, positions[0], DistanceUnits.Kilometers);
        OnPropertyChanged(nameof(Distance));
    }

    [RelayCommand]
    public void ClearCache () 
    {
        _dbManager.ReinitDb();
    }

}
