﻿using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PSOServerWebsite.Repositories;

public class LocationsRepository(HttpClient http)
{
    private static Task<IEnumerable<LocationModel>>? s_locations;

    public async Task<IEnumerable<LocationModel>> GetLocationsAsync()
    {
        s_locations ??= http.GetFromJsonAsync<IEnumerable<LocationModel>>("data/locations.json")!;
        return await s_locations;
    }
}

public class LocationModel
{
    public int Episode { get; set; }
    public LocationWhereModel[] Locations { get; set; } = [];
}

public class LocationWhereModel
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    [JsonPropertyName("ultimate")]
    public string? UltimateName { get; set; }
}
