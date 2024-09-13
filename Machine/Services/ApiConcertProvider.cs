using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Json;
using Android.Util;
using MetalMachine.Models;

namespace MetalMachine.Services;

public class ApiConcertProvider : IConcertProvider
{
    private string _apiKey;
    private int _startingPage;
    private Queue<Concert> _concerts;

    private Stopwatch _apiThrottling;

    private static string _baseUrlFormat = "https://api.setlist.fm/rest/1.0/user/{0}/attended?p={1}";

    public ApiConcertProvider() 
    {
        _apiKey = String.Empty;
        _startingPage = 1;
        _concerts = [];
        _apiThrottling = new Stopwatch();
    }
    public void Init(string initData) 
    {
        _apiKey = initData;
    }

    public Concert? GetNextConcert()
    {
        try 
        {
            return _concerts.Dequeue();
        }
        catch (InvalidOperationException ex)
        {
            Log.Debug("No more concerts to process", ex.Message);
            return null;
        }
    }

    public async Task<RetrySuggestion> PopulateConcertList(string? extraParameters = null)
    {
        if (!IsInit)
        {
            Log.Debug("API Provider not initialized yet!", extraParameters ?? String.Empty);
            return RetrySuggestion.WaitAndRetry;
        }

        // max 2 calls/second for setlist.fm policy, let's just wait more
        if (_apiThrottling.IsRunning && _apiThrottling.ElapsedMilliseconds < 1500) 
        {
            Log.Debug("Calls too close for API policy; wait!", _apiThrottling.ElapsedMilliseconds.ToString());
            return RetrySuggestion.WaitAndRetry;
        }
        else if (_apiThrottling.IsRunning)
        {
            _apiThrottling.Stop();
        }

        string finalUrl = String.Format(_baseUrlFormat, extraParameters, _startingPage);

        HttpClient httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        HttpResponseMessage resp = await httpClient.GetAsync(finalUrl);
        _apiThrottling.Start();

        if (!resp.IsSuccessStatusCode)
        {
            // end of available pages!
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) 
            {
                return RetrySuggestion.Stop;
            }
            Log.Warn("API call not succeeded!", resp.StatusCode.ToString());
            return RetrySuggestion.Retry;
        }

        // success
        object? decodedBody = await resp.Content.ReadFromJsonAsync<SetlistFmAttendedApiSchema>();

        int addedConcerts = 0;
        int expectedConcerts = 0;
        if (decodedBody is not null)
        {
            SetlistFmAttendedApiSchema? schemaResponse = decodedBody as SetlistFmAttendedApiSchema;
            if (schemaResponse is not null) 
            {
                expectedConcerts = schemaResponse.itemsPerPage ?? 0;
                foreach (SetlistFmSetlist sl in schemaResponse?.setlist ?? [])
                {
                    Location cLoc;
                    if (sl?.venue?.city?.coords?.lat is not null && sl?.venue?.city?.coords?.lon is not null) 
                    {
                        cLoc = new Location(
                                sl?.venue?.city?.coords?.lat ?? 0,
                                sl?.venue?.city?.coords?.lon ?? 0
                        ); 
                    }
                    else 
                    {
                        cLoc = (await DependencyService.Get<IGeocoding>()?.GetLocationsAsync($"{sl?.venue?.city?.name}, {sl?.venue?.city?.country?.name}"))?.FirstOrDefault();
                    }
                    _concerts.Enqueue(
                        new Concert(
                            sl.artist.name, 
                            cLoc,
                            DateTime.ParseExact(
                                sl?.eventDate ?? String.Empty, 
                                "dd-MM-yyyy", 
                                CultureInfo.InvariantCulture
                            ),
                            $"{sl?.venue?.city?.name}, {sl?.venue?.city?.country?.name}"
                        )
                    );
                    addedConcerts++;
                }
            }
        }
        if (addedConcerts > 0 && addedConcerts < expectedConcerts)
        {
            // page has less than normal number of concerts
            // reached the end?
            return RetrySuggestion.Stop;
        }
        else if (addedConcerts == 0)
        {
            // unsure what to do here; valid response but 0
            // concerts is kind of undocumented
            return RetrySuggestion.WaitAndRetry;
        }
        _startingPage += 1;
        return RetrySuggestion.Good;
    }

    public string ApiKey => _apiKey;
    public bool IsInit => _apiKey != String.Empty;

}
