using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MetalMachine.Models;
using MetalMachine.Services;

namespace MetalMachine.ViewModels;

public partial class BaseViewModel : ObservableObject
{

    protected IDBManager _dbManager;
    protected IGeocoding _geocoding;
    protected IPreferences _prefs;
    protected User _currUser;
    protected Location _currLocation;

    public BaseViewModel (IDBManager dbManager, IGeocoding geo, IPreferences pref) 
    {
        _dbManager = dbManager;
        _geocoding = geo;
        _prefs = pref;

        _currUser = new User(String.Empty, -1);
        RefreshCurrentUser();
    }

    public string CsvProgress { get; set; }
    public bool CsvIsInProgress { get; set; }

    private void RefreshCurrentUser() 
    {
        if (_prefs is not null) 
        {
            string prefUser = _prefs.Get<string>(typeof(MetalPreferences.UserName).Name, String.Empty);
            long prefUserId = _prefs.Get<long>(typeof(MetalPreferences.UserId).Name, -1);
            double prefLat = _prefs.Get<double>(typeof(MetalPreferences.UserLatitude).Name, 0.0);
            double prefLon = _prefs.Get<double>(typeof(MetalPreferences.UserLongitude).Name, 0.0);
            _currUser = new User(prefUser, prefUserId);
            _currLocation = new Location(prefLat, prefLon);
        }
        OnPropertyChanged(nameof(CurrentUser));
        OnPropertyChanged(nameof(CurrentLocation));
    }

    public User CurrentUser { get { return _currUser; }
        set 
        {
            if (_currUser == value) 
            {
                return;
            }
            if (value != null && value?.Name != String.Empty) 
            {
                _currUser = value;
                _prefs.Set<string>(typeof(MetalPreferences.UserName).Name, value?.Name ?? String.Empty);
                _prefs.Set<long>(typeof(MetalPreferences.UserId).Name, value?.Id ?? -1);
                Location userLoc = _dbManager.GetUserLocation(value?.Name ?? String.Empty).Result ?? new Location(0,0);
                _prefs.Set<double>(typeof(MetalPreferences.UserLatitude).Name, userLoc.Latitude);
                _prefs.Set<double>(typeof(MetalPreferences.UserLongitude).Name, userLoc.Longitude);
                _currLocation = userLoc;
            }
            OnPropertyChanged(nameof(CurrentUser));
            OnPropertyChanged(nameof(CurrentLocation));
        } 
    }

    public Location CurrentLocation => _currLocation;

    public bool LoggedIn => CurrentUser.Name != String.Empty;

    [RelayCommand]
    protected void Logout() 
    {
        if (_prefs is not null) 
        {
            _prefs.Set<string>(typeof(MetalPreferences.UserName).Name, String.Empty);
            _prefs.Set<long>(typeof(MetalPreferences.UserId).Name, -1);
            _prefs.Set<double>(typeof(MetalPreferences.UserLatitude).Name, 0.0);
            _prefs.Set<double>(typeof(MetalPreferences.UserLongitude).Name, 0.0);
            _currUser = new User(String.Empty, -1);
            OnPropertyChanged(nameof(CurrentUser));
            OnPropertyChanged(nameof(CurrentLocation));
            OnPropertyChanged(nameof(LoggedIn));
        }
    }


    public virtual async Task OnAppearing() 
    {
        RefreshCurrentUser();
        await Task.CompletedTask;
    }

}
