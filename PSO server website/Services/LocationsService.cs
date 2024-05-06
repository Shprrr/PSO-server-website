using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PSOServerWebsite.Services;

public class LocationsService(HttpClient http)
{
    public async Task<IEnumerable<LocationModel>> GetLocationsAsync()
    {
        return (await http.GetFromJsonAsync<IEnumerable<LocationModel>>("data/locations.json"))!;
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
