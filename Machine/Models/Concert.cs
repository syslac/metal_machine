using System;

namespace MetalMachine.Models;

public class Concert
{
    public Concert(string nm, string add) 
    {
        Name = nm;
        Address = add;
    }
    public string Address {get; set;}
    public string Name {get; set;}


}
