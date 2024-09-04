
namespace MetalMachine.Models;

public class Concert
{
    public Concert(string nm, Location add, DateTime date, string? addName = null) 
    {
        Name = nm;
        Date = date;
        Address = add;
        AddressName = addName ?? String.Empty;
    }
    public DateTime Date {get; set;}
    public Location Address {get; set;}
    public string Name {get; set;}

    public string AddressName {get; set;}


}
