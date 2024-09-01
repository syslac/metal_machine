using System;

namespace MetalMachine.Models;

public class User
{
    private string _user_name;
    private string _user_location;

    public User (string name, string? location = null) 
    {
        _user_name = name;
        if (location is not null) 
        {
            _user_location = location;
        }
    }

    public string Name => _user_name;
    public string Location { get; set; }

    public override string ToString()
    {
        return Name;
    }

}
