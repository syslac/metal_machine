using System;
using MetalMachine.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Android.Util;
using System.Data.Common;
using System.Data;
using Microsoft.Maui.Platform;

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

    public async void AddAddress(string address, (float, float) coordinates)
    {
        if (conn is null) 
        {
            return;
        }
        try 
        {
            if (conn.State != ConnectionState.Open) 
            {
                conn.OpenAsync();
            }
        }
        catch (Exception ex) 
        {
            Log.Warn("CTOR", ex.Message);
        }
        string query = """
        INSERT INTO [locations] ([address], [latitude], [longitude])
        VALUES ($add, $lat, $lon);
        """;
        SqliteCommand comm = conn.CreateCommand();
        comm.CommandText = query;
        await comm.PrepareAsync();
        comm.Parameters.AddWithValue("$add", address);
        comm.Parameters.AddWithValue("$lat", coordinates.Item1);
        comm.Parameters.AddWithValue("$lon", coordinates.Item2);

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

    public async Task<(float, float)?> GetCoordinates(string address, bool acceptAmbiguous = true)
    {
        if (conn is null) 
        {
            await Task.CompletedTask;
            return null;
        }
        try 
        {
            if (conn.State != ConnectionState.Open) 
            {
                conn.OpenAsync();
            }
        }
        catch (Exception ex) 
        {
            Log.Warn("CTOR", ex.Message);
        }
        string query = "SELECT * FROM [locations];";// WHERE [address] LIKE '%'";// '$add';";
        SqliteCommand comm = conn.CreateCommand();
        comm.CommandText = query;
        await comm.PrepareAsync();
        //comm.Parameters.AddWithValue("$add", address);
        (float, float)? retVal;
        try 
        {
            DataTable dt = new DataTable();
            DbDataReader res = await comm.ExecuteReaderAsync();
            dt.Load(res);
            if (dt.Rows.Count == 0 || (dt.Rows.Count > 1 && !acceptAmbiguous)) 
            { 
                return null;
            }
            DataRow record = dt.Rows[0];
            retVal = ((float)record["lat"], (float)record["lon"]);
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
        try 
        {
            if (conn.State != ConnectionState.Open) 
            {
                conn.OpenAsync();
            }
        }
        catch (Exception ex) 
        {
            Log.Warn("CTOR", ex.Message);
        }
        string [] createQuery = ["""
            CREATE TABLE IF NOT EXISTS users (id INTEGER PRIMARY KEY AUTOINCREMENT, 
            name TEXT);
        """,
        """
            CREATE TABLE IF NOT EXISTS concerts (id INTEGER PRIMARY KEY AUTOINCREMENT, 
            user INTEGER,
            concert_name TEXT,
            concert_location INTEGER);
        """,
        """
            CREATE TABLE IF NOT EXISTS locations (id INTEGER PRIMARY KEY AUTOINCREMENT, 
            address TEXT,
            latitude REAL,
            longitude REAL);
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
}
