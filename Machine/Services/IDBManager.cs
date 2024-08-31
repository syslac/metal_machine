using MetalMachine.Models;

namespace MetalMachine.Services;

public interface IDBManager
{
    // populate db; parameter is path in case of SQLite, connection string otherwise
    public void InitTables();
    // returns (lat, lon), requires address, returns null if no matches
    // or if we have more than 1 result and acceptAmbiguous is false
    public Task<(float, float)?> GetCoordinates(string address, bool acceptAmbiguous = true);
    // store a new address, coordinates are always (lat, lon)
    public void AddAddress(string address, (float, float) coordinates);
    // get all concerts of a single user
    public Task<List<Concert>?> GetAllConcerts(string user);
    // get a single concert (best match) using a query string
    public Task<Concert?> FindConcert(string user, string searchString);
    // store a new concert
    public void AddConcert(string user, Concert concert);


}
