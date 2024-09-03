using System;

namespace MetalMachine.Models;

public class Concert
{
    public Concert(string nm, Location add, string? addName = null) 
    {
        Name = nm;
        Address = add;
        AddressName = addName ?? String.Empty;
    }
    public Location Address {get; set;}
    public string Name {get; set;}

    public string AddressName {get; set;}


}
