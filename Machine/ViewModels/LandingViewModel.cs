using System;
using CommunityToolkit.Mvvm.ComponentModel;
using MetalMachine.Services;

namespace MetalMachine.ViewModels;

public class LandingViewModel : BaseViewModel
{
    private string _user;
    private float _latitude;
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
        _dbManager.AddAddress("Test", (45.0f, 12.0f));
        (float, float)? res = await _dbManager.GetCoordinates("Test");
        if (res is not null) 
        {
            _latitude = res?.Item1 ?? 0;
            OnPropertyChanged(nameof(Latitude));
        }
    }

}
