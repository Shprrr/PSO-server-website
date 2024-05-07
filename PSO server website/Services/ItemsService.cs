using System.Net.Http.Json;

namespace PSOServerWebsite.Services;

public class ItemsService(HttpClient http)
{
    public async Task<IEnumerable<ItemModel>> GetItemsAsync()
    {
        Dictionary<string, string>? names = await http.GetFromJsonAsync<Dictionary<string, string>>("data/names-v4.json");
        return names!.Select(n => new ItemModel(n.Key, n.Value));
    }
}

public class ItemModel(string identifier, string name)
{
    public string Identifier { get; set; } = identifier;
    public string Name { get; set; } = name;

    public override string ToString() => $"{Name} ({Identifier})";
}
