using Android.OS;
using MetalMachine.Models;

namespace MetalMachine.Services;

public interface IDBManager
{
    // populate db; parameter is path in case of SQLite, connection string otherwise
    public void InitTables();

    // calls InitTables after cleaning up db
    public void ReinitDb();
    public void CloseConnection();
    public Task<List<User>> GetUsers();
    public Task<Location> GetUserLocation(string user);
    public Task<Location> UpdateUserLocation(string user, string location, IGeocoding? geo);
    public Task<long> RegisterUser(string user);
    // returns (lat, lon), requires address, returns null if no matches
    // or if we have more than 1 result and acceptAmbiguous is false
    public Task<(Location?, long?)> GetCoordinates(string address, bool acceptAmbiguous = true);
    // store a new address, coordinates are always (lat, lon)
    public Task<long> AddAddress(string address, Location coordinates);
    // get all concerts of a single user
    public Task<List<Concert>> GetAllConcerts(long user, string? band, string? year);
    // get a single concert (best match) using a query string
    public Task<Concert?> FindConcert(string user, string? band, DateTime? date);
    // store a new concert
    public Task<long> AddConcert(long user_id, Concert concert);


}
