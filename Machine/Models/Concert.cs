
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

    public bool? didTravelBetweenConcerts(Concert comp, Location userLocation) 
    {
        if (comp is null || comp.Date == new DateTime() || Date == new DateTime())
        {
            // something wrong here, 
            return null;
        }
        // same day, same location
        if (comp.Date == Date && comp.Address == Address)
        {
            return false;
        }
        // consecutive days, same location
        // estimate on min kms required to have a person go back home between days vs staying there
        if (comp.Date >= Date 
            && comp.Date - Date <= new TimeSpan(40, 0, 0)
            && comp.Address == Address
            && Location.CalculateDistance(Address, userLocation, DistanceUnits.Kilometers) > 50
        ) 
        {
            return false;
        }
        // consecutive days, same location (no assumption on ordering)
        if (comp.Date <= Date 
            && Date - comp.Date <= new TimeSpan(40, 0, 0)
            && comp.Address == Address
            && Location.CalculateDistance(Address, userLocation, DistanceUnits.Kilometers) > 50
        ) 
        {
            return false;
        }
        // all other cases
        return true;
    }
}
