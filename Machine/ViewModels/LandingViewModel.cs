using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MetalMachine.Services;

namespace MetalMachine.ViewModels;

public partial class LandingViewModel : BaseViewModel
{
    private string _user;
    private double _latitude;
    public LandingViewModel(IDBManager db) : base(db) 
    {
        _user = "Syslac";
        OnPropertyChanged(nameof(User));
    }

    public string User => _user;
    public string Latitude => String.Format("{0}", _latitude);

    public override async Task OnAppearing() 
    {
        await base.OnAppearing();
        _dbManager.AddAddress("Test", (45.5, 12.5));
        (double, double)? res = await _dbManager.GetCoordinates("Test");
        if (res is not null) 
        {
            _latitude = res?.Item1 ?? 0;
            OnPropertyChanged(nameof(Latitude));
        }
    }

    [RelayCommand]
    public void ClearCache () 
    {
        _dbManager.ReinitDb();
    }

}
