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

    public BaseViewModel (IDBManager dbManager, IGeocoding geo, IPreferences pref) 
    {
        _dbManager = dbManager;
        _geocoding = geo;
        _prefs = pref;

        _currUser = new User(String.Empty);
        RefreshCurrentUser();
    }

    private void RefreshCurrentUser() 
    {
        if (_prefs is not null) 
        {
            string prefUser = _prefs.Get<string>(typeof(MetalPreferences.UserName).Name, String.Empty);
            _currUser = new User(prefUser);
        }
        OnPropertyChanged(nameof(CurrentUser));
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
            }
            OnPropertyChanged(nameof(CurrentUser));
        } 
    }
    public bool LoggedIn => CurrentUser.Name != String.Empty;

    [RelayCommand]
    protected void Logout() 
    {
        if (_prefs is not null) 
        {
            _prefs.Set<string>(typeof(MetalPreferences.UserName).Name, String.Empty);
            _currUser = new User(String.Empty);
            OnPropertyChanged(nameof(CurrentUser));
            OnPropertyChanged(nameof(LoggedIn));
        }
    }


    public virtual async Task OnAppearing() 
    {
        RefreshCurrentUser();
        await Task.CompletedTask;
    }

}
