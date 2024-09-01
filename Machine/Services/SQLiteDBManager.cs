using System;
using MetalMachine.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Android.Util;
using System.Data.Common;
using System.Data;
using Microsoft.Maui.Platform;
using Javax.Sql;

namespace MetalMachine.Services;

public class SQLiteDBManager : IDBManager
{
    private SqliteConnection? conn;
    public SQLiteDBManager (string path) 
    {
        if (path is null || path == String.Empty) 
        {
            return;
        }
        string completePath = $"{FileSystem.AppDataDirectory}/{path}";
        conn = new SqliteConnection($"Data Source={completePath}");
        Log.Warn("CTOR", $"Data Source={completePath}");

        InitTables();
    }

    ~SQLiteDBManager() 
    {
        conn?.Close();
        conn = null;
    }

    private async void Preamble() 
    {
        if (conn is null) 
        {
            await Task.CompletedTask;
            Log.Warn("CTOR", "Null connection");
            return;
        }
        try 
        {
            if (conn.State != ConnectionState.Open) 
            {
                await conn.OpenAsync();
            }
        }
        catch (Exception ex) 
        {
            Log.Warn("CTOR", ex.Message);
        }

    }

    public async void AddAddress(string address, Location coordinates)
    {
        Preamble();

        string query = """
        INSERT INTO [locations] ([address], [latitude], [longitude])
        VALUES ($add, $lat, $lon);
        """;
        SqliteCommand comm = conn.CreateCommand();
        comm.CommandText = query;
        await comm.PrepareAsync();
        comm.Parameters.AddWithValue("$add", address);
        comm.Parameters.AddWithValue("$lat", coordinates.Latitude);
        comm.Parameters.AddWithValue("$lon", coordinates.Longitude);

        try 
        {
            int rowsInserted = await comm.ExecuteNonQueryAsync();
            if (rowsInserted < 1) 
            {
                Log.Warn($"AddAddress - {query}", "No rows inserted");
            }
        }
        catch (Exception ex) 
        {
            Log.Warn($"AddAddress - {query}", ex.Message);
        }
        finally 
        {
            await conn.CloseAsync();
        }
    }

    public async void AddConcert(string user, Concert concert)
    {
        throw new NotImplementedException();
    }

    public async Task<Concert?> FindConcert(string user, string searchString)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Concert>?> GetAllConcerts(string user)
    {
        throw new NotImplementedException();
    }

    public async Task<Location?> GetCoordinates(string address, bool acceptAmbiguous = true)
    {
        Preamble();

        string query = "SELECT * FROM [locations] WHERE [address] LIKE @add;";
        SqliteCommand comm = conn.CreateCommand();
        comm.CommandText = query;
        await comm.PrepareAsync();
        comm.Parameters.AddWithValue("@add", address);
        Location retVal = new Location();
        try 
        {
            DataTable dt = new DataTable();
            DbDataReader res = await comm.ExecuteReaderAsync();
            dt.Load(res);
            if (dt.Rows.Count == 0 
                || (dt.Rows.Count > 1 && !acceptAmbiguous)
                ) 
            { 
                return null;
            }
            DataRow record = dt.Rows[0];
            retVal.Latitude = record.Field<double>("latitude");
            retVal.Longitude = record.Field<double>("longitude");
        }
        catch (Exception ex) 
        {
            Log.Warn($"GetCoordinates - {query}", ex.Message);
            retVal = null;
        }
        finally 
        {
            await conn.CloseAsync();
        }
        return retVal;
    }

    public async Task<string> GetUserLocation(string user)
    {
        Preamble();

        string query = """
            SELECT [users].[user_name], [locations].[address] 
            FROM [users] 
            LEFT JOIN [locations] ON [users].[user_location_id] = [locations].[id]
            WHERE [users].[user_name] LIKE @name;
            """;
        SqliteCommand comm = conn.CreateCommand();
        comm.CommandText = query;
        await comm.PrepareAsync();
        comm.Parameters.AddWithValue("@name", user);
        string retVal = String.Empty;
        try 
        {
            DataTable dt = new DataTable();
            DbDataReader res = await comm.ExecuteReaderAsync();
            dt.Load(res);
            if (dt.Rows.Count == 0) 
            { 
                return null;
            }
            DataRow record = dt.Rows[0];
            retVal = record.Field<string>("address");
        }
        catch (Exception ex) 
        {
            Log.Warn($"GetCoordinates - {query}", ex.Message);
            retVal = null;
        }
        finally 
        {
            await conn.CloseAsync();
        }
        return retVal;
    }

    public async Task<List<User>> GetUsers()
    {
        Preamble();

        string query = "SELECT * FROM [users];";
        SqliteCommand comm = conn.CreateCommand();
        comm.CommandText = query;
        await comm.PrepareAsync();
        List<User> retVal = [];
        try 
        {
            DataTable dt = new DataTable();
            DbDataReader res = await comm.ExecuteReaderAsync();
            dt.Load(res);
            foreach (DataRow record in dt.Rows) 
            {
                retVal.Add(new User(record.Field<string>("user_name")));
            }
        }
        catch (Exception ex) 
        {
            Log.Warn($"GetCoordinates - {query}", ex.Message);
            retVal = null;
        }
        finally 
        {
            await conn.CloseAsync();
        }
        return retVal;
    }

    public async void InitTables()
    {
        Preamble();
        
        string [] createQuery = ["""
            CREATE TABLE IF NOT EXISTS users ([id] INTEGER PRIMARY KEY AUTOINCREMENT, 
            [user_name] TEXT UNIQUE,
            [user_location_id] INTEGER);
        """,
        """
            CREATE TABLE IF NOT EXISTS concerts ([id] INTEGER PRIMARY KEY AUTOINCREMENT, 
            [user_id] INTEGER,
            [concert_name] TEXT,
            [concert_location_id] INTEGER);
        """,
        """
            CREATE TABLE IF NOT EXISTS locations ([id] INTEGER PRIMARY KEY AUTOINCREMENT, 
            [address] TEXT UNIQUE,
            [latitude] REAL,
            [longitude] REAL);
        """];
        foreach (string query in createQuery) 
        {
            SqliteCommand comm = conn.CreateCommand();
            comm.CommandText = query;
            try 
            {
                int rowsInserted = await comm.ExecuteNonQueryAsync();
            }
            catch (Exception ex) 
            {
                Log.Warn($"InitTables - init-ing {query}", ex.Message);
            }
        }
        await conn.CloseAsync();
    }

    public async void RegisterUser(string user)
    {
        Preamble();

        string query = """
        INSERT INTO [users] ([user_name])
        VALUES (@name);
        """;
        SqliteCommand comm = conn.CreateCommand();
        comm.CommandText = query;
        await comm.PrepareAsync();
        comm.Parameters.AddWithValue("@name", user);

        try 
        {
            int rowsInserted = await comm.ExecuteNonQueryAsync();
            if (rowsInserted < 1) 
            {
                Log.Warn($"RegisterUser - {query}", "No rows inserted");
            }
        }
        catch (Exception ex) 
        {
            Log.Warn($"RegisterUser - {query}", ex.Message);
        }
        finally 
        {
            await conn.CloseAsync();
        }
    }

    public async void ReinitDb() 
    {
        Preamble();

        string [] deleteQuery = ["""
            DROP TABLE users;
        """,
        """
            DROP TABLE concerts;
        """,
        """
            DROP TABLE locations;
        """];
        foreach (string query in deleteQuery) 
        {
            SqliteCommand comm = conn.CreateCommand();
            comm.CommandText = query;
            try 
            {
                int rowsInserted = await comm.ExecuteNonQueryAsync();
            }
            catch (Exception ex) 
            {
                Log.Warn($"ResetDb - delete-ing {query}", ex.Message);
            }
        }
        await conn.CloseAsync();

        InitTables();
    }

    public async Task<Location> UpdateUserLocation(string user, string location, IGeocoding? geocoding)
    {
        Preamble();

        string query = """
            SELECT *
            FROM [locations] 
            WHERE [address] LIKE @loc;
            """;
        SqliteCommand comm = conn.CreateCommand();
        comm.CommandText = query;
        await comm.PrepareAsync();
        comm.Parameters.AddWithValue("@loc", location);
        Location toRet = new Location();
        try 
        {
            DataTable dt = new DataTable();
            DbDataReader res = await comm.ExecuteReaderAsync();
            dt.Load(res);
            long idToEdit = 0;
            if (dt.Rows.Count == 0) 
            { 
                // not found, need to insert new location
                if (geocoding is not null)
                {
                    Location geocodedLoc = (await geocoding.GetLocationsAsync(location)).FirstOrDefault();
                    toRet.Latitude = geocodedLoc.Latitude;
                    toRet.Longitude = geocodedLoc.Longitude;

                    query = """
                    INSERT INTO [locations] ([address],[latitude],[longitude])
                    VALUES (@add,@lat,@lon);
                    """;
                    SqliteCommand insertComm = conn.CreateCommand();
                    insertComm.CommandText = query;
                    await insertComm.PrepareAsync();
                    insertComm.Parameters.AddWithValue("@add", location);
                    insertComm.Parameters.AddWithValue("@lat", geocodedLoc.Latitude);
                    insertComm.Parameters.AddWithValue("@lon", geocodedLoc.Longitude);

                    int rowsInserted = await insertComm.ExecuteNonQueryAsync();
                    if (rowsInserted < 1) 
                    {
                        Log.Warn($"UpdateUserLocation - {query}", "No rows inserted");
                    }
                    query = "select last_insert_rowid();";
                    comm.CommandText = query;
                    idToEdit = (long)comm.ExecuteScalar();
                }
            }
            else 
            {
                // found, need to edit user
                idToEdit = dt.Rows[0].Field<long>("id");
                toRet.Latitude = dt.Rows[0].Field<double>("latitude");
                toRet.Longitude = dt.Rows[0].Field<double>("longitude");
            }
            query = """
                UPDATE 
                [users] 
                SET [user_location_id] = @lId
                WHERE [user_name] = @user;
                """;
            comm.CommandText = query;
            await comm.PrepareAsync();
            comm.Parameters.AddWithValue("@lId", idToEdit);
            comm.Parameters.AddWithValue("@user", user);
            int rowsUpdated = await comm.ExecuteNonQueryAsync();
            if (rowsUpdated < 1) 
            {
                Log.Warn($"UpdateUserLocation - {query}", "User not affected");
            }
        }
        catch (Exception ex) 
        {
            Log.Warn($"GetCoordinates - {query}", ex.Message);
        }
        finally 
        {
            await conn.CloseAsync();
        }
        return toRet;
    }
}
