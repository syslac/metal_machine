using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MetalMachine.Services;

namespace MetalMachine.ViewModels;

public class BaseViewModel : ObservableObject
{

    protected IDBManager _dbManager;
    protected IGeocoding _geocoding;
    protected IPreferences _prefs;

    public BaseViewModel (IDBManager dbManager, IGeocoding geo, IPreferences pref) 
    {
        _dbManager = dbManager;
        _geocoding = geo;
        _prefs = pref;
    }

    public virtual async Task OnAppearing() 
    {
        await Task.CompletedTask;
    }

}
