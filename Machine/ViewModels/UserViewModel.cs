using System;
using MetalMachine.Services;

namespace MetalMachine.ViewModels;

public class UserViewModel : BaseViewModel
{
    private string _currUser;

    public UserViewModel (IDBManager db, IGeocoding g, IPreferences p) : base (db, g, p) 
    {
        if (_prefs is not null) 
        {
            string prefUser = _prefs.Get<string>(typeof(MetalPreferences.UserName).Name, String.Empty);
            if (prefUser != String.Empty) 
            {
                _currUser = prefUser;
            }
        }
    }

    public string CurrentUser => _currUser;

}
