using System;

namespace MetalMachine.Models;

public class User
{
    private long _id;
    private string _user_name;
    private Location _user_location;

    public User (string name, long id, Location? location = null) 
    {
        _user_name = name;
        _id = id;
        if (location is not null) 
        {
            _user_location = location;
        }
    }

    public long Id => _id;
    public string Name => _user_name;
    public Location Location { get; set; }

    public override string ToString()
    {
        return Name;
    }

}
