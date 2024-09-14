using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Java.Util;
using MetalMachine.Models;
using MetalMachine.Services;

namespace MetalMachine.ViewModels;

public partial class UserViewModel : BaseViewModel
{
    private List<User> _users;

    public UserViewModel (IDBManager db, IGeocoding g, IPreferences p, IConcertProvider c, IMessenger m) : base (db, g, p, c, m) 
    {
        _users = [];
        SelectingExisting = true;

    }

    public override async Task OnAppearing() 
    {
        await base.OnAppearing();
        if (_dbManager is not null) 
        {
            _users = await _dbManager.GetUsers();
            OnPropertyChanged(nameof(AvailableUsers));
        }
    }

    public ObservableCollection<User> AvailableUsers => new ObservableCollection<User>(_users);
    public string NewUser { get; set; }
    public string NewLocation { get; set; }

    public bool SelectingExisting { get; set; }
    public string CurrentLat => CurrentLocation.Latitude.ToString("n2");
    public string CurrentLon => CurrentLocation.Longitude.ToString("n2");

    [RelayCommand]
    private void SwitchExistingNew()
    {
        SelectingExisting = !SelectingExisting;
        OnPropertyChanged(nameof(SelectingExisting));
    } 

    [RelayCommand]
    private async Task RegisterUser()
    {
        if (_dbManager is not null)
        {
            long newId = await _dbManager.RegisterUser(NewUser);
            _users = await _dbManager.GetUsers();
            OnPropertyChanged(nameof(AvailableUsers));

            CurrentUser = new User(NewUser, newId);
        }
        NewUser = String.Empty;
        SelectingExisting = !SelectingExisting;
        OnPropertyChanged(nameof(SelectingExisting));
        OnPropertyChanged(nameof(CurrentUser));
        OnPropertyChanged(nameof(NewUser));
        OnPropertyChanged(nameof(LoggedIn));
    } 

    [RelayCommand]
    private async Task UpdateLocation()
    {
        if (_dbManager is not null)
        {
            Location storedLocation = await _dbManager.UpdateUserLocation(CurrentUser.Name, NewLocation, _geocoding);
            _currLocation = storedLocation;
            _prefs.Set<double>(typeof(MetalPreferences.UserLatitude).Name, storedLocation.Latitude);
            _prefs.Set<double>(typeof(MetalPreferences.UserLongitude).Name, storedLocation.Longitude);
            OnPropertyChanged(nameof(CurrentLat));
            OnPropertyChanged(nameof(CurrentLon));
        }
    } 

}
