using System.Text.Json;
using System.Text.Json.Serialization;

namespace PSOServerWebsite.Repositories;

public class ConfigurationRepository(HttpClient http)
{
    private static readonly JsonSerializerOptions s_options = new() { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };
    private static string? s_configJson = null;

    public async Task<ConfigModel> GetConfigAsync()
    {
        s_configJson ??= (await http.GetStringAsync("data/config.json")).FixJson();
        return JsonSerializer.Deserialize<ConfigModel>(s_configJson, s_options)!;
    }
}

public class ConfigModel
{
    // Reward 0 = Dorphon  Reward 1 = Sand Rappy  Reward 2 = Zu  Reward 4 = BPDD2
    public string[][][] QuestF95EResultItems { get; set; } = default!;
    public QuestF95FModel[] QuestF95FResultItems { get; set; } = default!;
    public string[] SecretLotteryResultItems { get; set; } = default!;
    public QuestF960Model[] QuestF960SuccessResultItems { get; set; } = default!;
    public Dictionary<string, string[]> QuestF960FailureResultItems { get; set; } = default!;
    public double ServerGlobalDropRateMultiplier { get; set; } = 1;
    public QuestF95EModel ConvertQuestF95EResultItems()
    {
        QuestF95EModel questF95E = new();
        questF95E.Dorphon.Normal = QuestF95EResultItems[0][0];
        questF95E.Dorphon.Hard = QuestF95EResultItems[0][1];
        questF95E.Dorphon.VeryHard = QuestF95EResultItems[0][2];
        questF95E.Dorphon.Ultimate = QuestF95EResultItems[0][3];
        questF95E.SandRappy.Normal = QuestF95EResultItems[1][0];
        questF95E.SandRappy.Hard = QuestF95EResultItems[1][1];
        questF95E.SandRappy.VeryHard = QuestF95EResultItems[1][2];
        questF95E.SandRappy.Ultimate = QuestF95EResultItems[1][3];
        questF95E.Zu.Normal = QuestF95EResultItems[2][0];
        questF95E.Zu.Hard = QuestF95EResultItems[2][1];
        questF95E.Zu.VeryHard = QuestF95EResultItems[2][2];
        questF95E.Zu.Ultimate = QuestF95EResultItems[2][3];
        questF95E.BPDD2.Normal = QuestF95EResultItems[4][0];
        questF95E.BPDD2.Hard = QuestF95EResultItems[4][1];
        questF95E.BPDD2.VeryHard = QuestF95EResultItems[4][2];
        questF95E.BPDD2.Ultimate = QuestF95EResultItems[4][3];
        return questF95E;
    }
    public string ApplyDropRateMultiplier(string probability)
    {
        if (ServerGlobalDropRateMultiplier == 1)
            return probability;

        string[] probabilityNumbers = probability.Split('/');
        return $"{double.Parse(probabilityNumbers[0]) * ServerGlobalDropRateMultiplier}/{double.Parse(probabilityNumbers[1])}";
    }
}

public class QuestF95EModel
{
    public QuestF95EType Dorphon { get; set; } = new();
    public QuestF95EType SandRappy { get; set; } = new();
    public QuestF95EType Zu { get; set; } = new();
    public QuestF95EType BPDD2 { get; set; } = new();

    public IEnumerable<NamedObject<QuestF95EType>> Types()
    {
        yield return new("Black Paper's Dangerous Deal 1 Dorphon route", Dorphon);
        yield return new("Black Paper's Dangerous Deal 1 Sand Rappy route", SandRappy);
        yield return new("Black Paper's Dangerous Deal 1 Zu route", Zu);
        yield return new("Black Paper's Dangerous Deal 2", BPDD2);
    }
}

public class QuestF95EType
{
    public string[] Normal { get; set; } = [];
    public string[] Hard { get; set; } = [];
    public string[] VeryHard { get; set; } = [];
    public string[] Ultimate { get; set; } = [];

    public IEnumerable<NamedObject<string[]>> Difficulties()
    {
        yield return new("Normal", Normal);
        yield return new("Hard", Hard);
        yield return new("Very Hard", VeryHard);
        yield return new("Ultimate", Ultimate);
    }
}

[JsonConverter(typeof(QuestF95FModelConverter))]
public class QuestF95FModel(int numPhotonTickets, string itemDescription)
{
    public int NumPhotonTickets { get; set; } = numPhotonTickets;

    public string ItemDescription { get; set; } = itemDescription;
}
public class QuestF95FModelConverter : JsonConverter<QuestF95FModel>
{
    public override QuestF95FModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Read();// StartArray
        int numPhotonTickets = reader.GetInt32();
        reader.Read();
        string itemDescription = reader.GetString()!;
        reader.Read();
        return new QuestF95FModel(numPhotonTickets, itemDescription);
    }

    public override void Write(Utf8JsonWriter writer, QuestF95FModel value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber(nameof(value.NumPhotonTickets), value.NumPhotonTickets);
        writer.WriteString(nameof(value.ItemDescription), value.ItemDescription);
        writer.WriteEndObject();
    }
}
public class QuestF960Model
{
    public int MesetaCost { get; set; }
    public string BaseProbability { get; set; } = default!;
    public string ProbabilityUpgrade { get; set; } = default!;
    [JsonInclude, JsonExtensionData]
    private Dictionary<string, JsonElement> _ItemsByDay { get; set; } = [];
    public Dictionary<string, string[]> ItemsByDay => _ItemsByDay.ToDictionary(i => i.Key, i => i.Value.EnumerateArray().Select(v => v.GetString() ?? "None").ToArray());
}
