using System.Text.Json;
using System.Text.Json.Serialization;

namespace PSOServerWebsite.Services;

public class RareDropsService(HttpClient http)
{
    private static readonly JsonSerializerOptions s_options = new() { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };

    public async Task<RareDropModel> GetRareDropsAsync()
    {
        string json = (await http.GetStringAsync("data/rare-table-v4.json")).FixJson();
        return JsonSerializer.Deserialize<RareDropModel>(json, s_options)!;
    }
}

// The overall data structure is:
// {GameMode: {Episode: {Difficulty: {SectionID: {Where: [RareSpec, ...]}}}}}
// Most of the keys should be self-explanatory. `Where` can be an enemy type
// (which matches the EnemyType enum), or "Box-" suffixed with an area name.
// RareSpec entries are lists of 2 items of the form [Probability, ItemDesc].
public class RareDropModel
{
    public GameModeModel Normal { get; set; } = new();
}

public class GameModeModel
{
    public EpisodeModel Episode1 { get; set; } = new();
    public EpisodeModel Episode2 { get; set; } = new();
    public EpisodeModel Episode4 { get; set; } = new();

    public IEnumerable<NamedObject<EpisodeModel>> Episodes()
    {
        yield return new("Episode 1", Episode1);
        yield return new("Episode 2", Episode2);
        yield return new("Episode 4", Episode4);
    }
}

public class EpisodeModel
{
    public DifficultyModel Normal { get; set; } = new();
    public DifficultyModel Hard { get; set; } = new();
    public DifficultyModel VeryHard { get; set; } = new();
    public DifficultyModel Ultimate { get; set; } = new();

    public IEnumerable<NamedObject<DifficultyModel>> Difficulties()
    {
        yield return new("Normal", Normal);
        yield return new("Hard", Hard);
        yield return new("Very Hard", VeryHard);
        yield return new("Ultimate", Ultimate);
    }
}

public class DifficultyModel
{
    public SectionIdModel Viridia { get; set; } = [];
    public SectionIdModel Greennill { get; set; } = [];
    public SectionIdModel Skyly { get; set; } = [];
    public SectionIdModel Bluefull { get; set; } = [];
    public SectionIdModel Purplenum { get; set; } = [];
    public SectionIdModel Pinkal { get; set; } = [];
    public SectionIdModel Redria { get; set; } = [];
    public SectionIdModel Oran { get; set; } = [];
    public SectionIdModel Yellowboze { get; set; } = [];
    public SectionIdModel Whitill { get; set; } = [];

    public IEnumerable<string> Where()
    {
        return Viridia.Keys.Union(Greennill.Keys.Union(Skyly.Keys.Union(Bluefull.Keys.Union(Purplenum.Keys.Union(Pinkal.Keys.Union(Redria.Keys.Union(Oran.Keys.Union(Yellowboze.Keys.Union(Whitill.Keys)))))))));
    }

    public IEnumerable<SectionIdModel> SectionsId()
    {
        yield return Viridia;
        yield return Greennill;
        yield return Skyly;
        yield return Bluefull;
        yield return Purplenum;
        yield return Pinkal;
        yield return Redria;
        yield return Oran;
        yield return Yellowboze;
        yield return Whitill;
    }
}

public class SectionIdModel : Dictionary<string, RareSpecificationModel[]>
{
}

[JsonConverter(typeof(RareSpecificationModelConverter))]
public class RareSpecificationModel(string probability, string itemDescription)
{
    /// <summary>
    /// <see cref="Probability" /> may be a 32-bit integer specifying the relative frequency of
    /// finding the item, out of 2^32 (so 0x80000000 means a 50% chance), or it may
    /// be a fraction represented as a string (e.g. "3/32").
    /// </summary>
    public string Probability { get; set; } = probability;

    /// <summary>
    /// <see cref="ItemDescription" /> may be a
    /// textual description of the item, the item's data specified as a hex string,
    /// or an integer specifying the 3-byte item code (in this case, the item code
    /// may be specified in hex, like 0x009D00). If an item has any extended
    /// attributes specified (that is, if there are any nonzero bytes in the item's
    /// data beyond the first three bytes), then the standard random attribute
    /// logic is disabled for that item and it will drop exactly as specified.
    /// </summary>
    public string ItemDescription { get; set; } = itemDescription;
}
public class RareSpecificationModelConverter : JsonConverter<RareSpecificationModel>
{
    public override RareSpecificationModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Read();// StartArray
        string probability = reader.GetString()!;
        reader.Read();
        string itemDescription = reader.GetString()!;
        reader.Read();
        return new RareSpecificationModel(probability, itemDescription);
    }

    public override void Write(Utf8JsonWriter writer, RareSpecificationModel value, JsonSerializerOptions options) => throw new NotImplementedException();
}

public class NamedObject<T>(string name, T? value)
{
    public string Name { get; set; } = name;

    public T? Value { get; set; } = value;
}
