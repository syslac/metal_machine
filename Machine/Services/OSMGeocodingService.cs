using System;
using System.Net.Http.Json;

namespace MetalMachine.Services;

public class NominatimSingleResult 
{
    public long? place_id;
    public string? licence;
    public string? osm_type;
    public long? osm_id;
    public double? lat;
    public double? lon;
    public string? category;
    public string? type;
    public int? place_rank;
    public double? importance;
    public string? addresstype;
    public string? name;
    public string? display_name;
    public double[]? boundingbox;

    public (double, double)? Coordinates => (lat is not null && lon is not null) ? (lat ?? 0, lon ?? 0) : null;
};

public class NominatimResponseFormat 
{
    public NominatimSingleResult[] _Results;

    public int ResultLen => _Results.Length;
    public NominatimSingleResult FirstResult => _Results[0];
};

public class OSMGeocodingService : IGeocoding
{
    private string QueryUrl(string address) 
    {
        return string.Format("https://nominatim.openstreetmap.org/search.php?q={0}&format=jsonv2", address);
    }

    public Task<IEnumerable<Placemark>> GetPlacemarksAsync(double latitude, double longitude)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Location>> GetLocationsAsync(string address)
    {
        var client = new HttpClient();
        List<Location> retList = [];
        HttpResponseMessage result = await client.GetAsync(QueryUrl(address));
        if (result is not null && result.IsSuccessStatusCode) 
        {
            object? decodedJson = await result.Content.ReadFromJsonAsync(typeof(NominatimResponseFormat));
            if (decodedJson is not null) 
            {
                NominatimResponseFormat decodedRes = (NominatimResponseFormat)decodedJson;
                if (decodedRes is not null && decodedRes.ResultLen > 0) 
                {
                    retList.Add(new Location(decodedRes.FirstResult.lat ?? 0, decodedRes.FirstResult.lon ?? 0));
                }
            }
        }
        return retList;
    }
}
