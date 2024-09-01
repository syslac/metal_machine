using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Java.Util;
using MetalMachine.Models;
using MetalMachine.Services;

namespace MetalMachine.ViewModels;

public partial class UserViewModel : BaseViewModel
{
    private List<User> _users;

    public UserViewModel (IDBManager db, IGeocoding g, IPreferences p) : base (db, g, p) 
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

    [RelayCommand]
    private void SwitchExistingNew()
    {
        SelectingExisting = !SelectingExisting;
        OnPropertyChanged(nameof(SelectingExisting));
    } 

    [RelayCommand]
    private async void RegisterUser()
    {
        if (_dbManager is not null)
        {
            _dbManager.RegisterUser(NewUser);
            _users = await _dbManager.GetUsers();
            OnPropertyChanged(nameof(AvailableUsers));
        }
        _currUser = new User(NewUser);
        NewUser = String.Empty;
        SelectingExisting = !SelectingExisting;
        OnPropertyChanged(nameof(SelectingExisting));
        OnPropertyChanged(nameof(CurrentUser));
        OnPropertyChanged(nameof(NewUser));
        OnPropertyChanged(nameof(LoggedIn));
    } 

    [RelayCommand]
    private async void UpdateLocation()
    {
        if (_dbManager is not null)
        {
            Location storedLocation = await _dbManager.UpdateUserLocation(CurrentUser.Name, NewLocation, _geocoding);
            _prefs.Set<double>(typeof(MetalPreferences.UserLatitude).Name, storedLocation.Latitude);
            _prefs.Set<double>(typeof(MetalPreferences.UserLongitude).Name, storedLocation.Longitude);
            OnPropertyChanged(nameof(CurrentLocation));
        }
    } 

}
