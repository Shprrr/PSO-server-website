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

public class ItemModel(string id, string name)
{
    public string Id { get; set; } = id;
    public string Name { get; set; } = name;

    public override string ToString() => $"{Name} ({Id})";
}
