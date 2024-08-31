using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MetalMachine.Services;

namespace MetalMachine.ViewModels;

public class BaseViewModel : ObservableObject
{

    protected IDBManager _dbManager;

    public BaseViewModel (IDBManager dbManager) 
    {
        _dbManager = dbManager;
    }

    public virtual async Task OnAppearing() 
    {
        await Task.CompletedTask;
    }

}
