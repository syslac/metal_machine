using System;

namespace MetalMachine.Models;

internal class SetlistFmCoords
{
    public double? lat { get; set; }
    public double? lon { get; set; }
};

internal class SetlistFmCountry
{
    public string? code { get; set; }
    public string? name { get; set; }
};

internal class SetlistFmSong 
{
    public string? name { get; set; }
    public bool? tape { get; set; }
    public object? cover { get; set; }
};

internal class SetlistFmCity 
{
    public string? id { get; set; }
    public string? name { get; set; }
    public string? state { get; set; }
    public string? stateCode { get; set; }
    public SetlistFmCoords? coords { get; set; }
    public SetlistFmCountry? country { get; set; }
};

internal class SetlistFmSingleSet 
{
    public int? encore { get; set; }
    public SetlistFmSong[]? song { get; set; }
};

internal class SetlistFmArtist 
{
    public string? mbid { get; set; }
    public string? name { get; set; }
    public string? sortName { get; set; }
    public string? disambiguation { get; set; }
    public string? url { get; set; }
};

internal class SetlistFmVenue
{
    public string? id { get; set; }
    public string? name { get; set; }
    public SetlistFmCity? city { get; set; }
    public string? url { get; set; }
};

internal class SetlistFmSets
{
    public SetlistFmSingleSet[]? set { get; set; }
};

internal class SetlistFmSetlist 
{
    public string? id { get; set; }
    public string? versionId { get; set; }
    public string? eventDate { get; set; }
    public string? lastUpdated { get; set; }
    public SetlistFmArtist? artist { get; set; }
    public SetlistFmVenue? venue { get; set; }
    public SetlistFmSets? sets { get; set; }
    public string? info { get; set; }
    public string? url { get; set; }

};


internal class SetlistFmAttendedApiSchema 
{
    public string? type { get; set; }
    public int? itemsPerPage { get; set; }
    public int? page { get; set; }
    public int? total { get; set; }
    public SetlistFmSetlist[]? setlist { get; set; }

};
