using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PSOServerWebsite.Repositories;

public class LocationsRepository(HttpClient http)
{
    private static readonly Dictionary<string, Task<IEnumerable<LocationModel>>> s_locations = [];

    public async Task<IEnumerable<LocationModel>> GetLocationsAsync(string? version = null)
    {
        string filename = "data/locations.json";
        if (version == null)
            version = "current";
        else
            filename = $"data/locations-{version}.json";

        if (!s_locations.TryGetValue(version, out Task<IEnumerable<LocationModel>>? locations))
        {
            locations = http.GetFromJsonAsync<IEnumerable<LocationModel>>(filename)!;
            s_locations.Add(version, locations);
        }

        return await locations;
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
