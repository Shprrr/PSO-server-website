using PSOServerWebsite.Repositories;

namespace PSOServerWebsite.Services;

public class DropsLocationsService(RareDropsRepository rareDropsRepository, LocationsRepository locationsRepository, ItemsRepository itemsRepository, ConfigurationRepository configurationRepository)
{
    public async Task<IEnumerable<DropLocationModel>> GetDropsLocationsAsync()
    {
        RareDropModel rareDrops = await rareDropsRepository.GetRareDropsAsync();
        var locations = (await locationsRepository.GetLocationsAsync())
            .ToDictionary(l => l.Episode, l => l.Locations.Index()
                .ToDictionary(l => l.Item.Id, l => (l.Index, l.Item.Name, UltimateName: string.IsNullOrEmpty(l.Item.UltimateName) ? l.Item.Name : l.Item.UltimateName)));
        var itemsByName = (await itemsRepository.GetItemsAsync())
            .GroupBy(i => i.Name, StringComparer.InvariantCultureIgnoreCase)
            .ToDictionary(i => i.Key, i => i.First().Identifier, StringComparer.InvariantCultureIgnoreCase);
        ConfigModel config = await configurationRepository.GetConfigAsync();

        List<DropLocationModel> dropsLocations = [..rareDrops.Normal.Episodes()
            .SelectMany(episode => episode.Value.Difficulties()
            .SelectMany(difficulty => difficulty.Value.SectionsId()
            .SelectMany(sectionId => sectionId.Value.SelectMany(dl => dl.Value,
                (dropLocation, itemProbability) => CreateDropLocationModel(episode, difficulty, sectionId, dropLocation, itemProbability, locations, config)))))];

        dropsLocations.AddRange(config.ConvertQuestF95EResultItems().Types()
            .SelectMany(t => t.Value.Difficulties(), (type, difficulties) => (type, difficulties))
            .SelectMany(q => q.difficulties.Value, (q, item) =>
            {
                int chances = 1;
                if (q.type.Name.StartsWith("Black Paper's Dangerous Deal 1"))
                    chances = q.difficulties.Name switch
                    {
                        "Hard" => 2,
                        "Very Hard" => 3,
                        "Ultimate" => 4,
                        _ => 1
                    };
                if (q.type.Name == "Black Paper's Dangerous Deal 2" && (q.difficulties.Name == "Very Hard" || q.difficulties.Name == "Ultimate"))
                    chances = 2;
                int order = q.type.Name switch
                {
                    "Black Paper's Dangerous Deal 1 Dorphon route" => 0,
                    "Black Paper's Dangerous Deal 1 Sand Rappy route" => 1,
                    "Black Paper's Dangerous Deal 1 Zu route" => 2,
                    "Black Paper's Dangerous Deal 2" => 4,
                    _ => throw new InvalidOperationException()
                };
                return new DropLocationModel("Episode 4", q.difficulties.Name, "", q.type.Name, order, item, $"{chances}/{q.difficulties.Value.Length}");
            }));

        dropsLocations.AddRange(config.QuestF95FResultItems.Select(q => new DropLocationModel("Episode 1", "", "", $"Gallon's Plan exchange {q.NumPhotonTickets} Photon Ticket", 0, q.ItemDescription, "1/1")));

        dropsLocations.AddRange(config.SecretLotteryResultItems.Select(i => new DropLocationModel("Episode 1", "", "", $"Good Luck! exchange Secret Ticket", 0, i, $"1/{config.SecretLotteryResultItems.Length}")));

        dropsLocations.AddRange(config.QuestF960SuccessResultItems.SelectMany(q => q.ItemsByDay.SelectMany(i => i.Value, (q, i) => (q.Key, i)).Select(i => new DropLocationModel("", "", "", $"Coren {q.MesetaCost:N0} Meseta {i.Key}", -q.MesetaCost, itemsByName[i.i], $"1/{q.ItemsByDay[i.Key].Length}"))));

        dropsLocations.AddRange(config.QuestF960FailureResultItems.SelectMany(q => q.Value.Select(i => new DropLocationModel("", "", "", $"Coren Failure {q.Key}", 0, itemsByName[i.Replace(" x1", "")], $"1/{q.Value.Length}"))));

        dropsLocations.AddRange([
            new DropLocationModel("Episode 1", "Normal", "", "Governement Quests Hunter 1‑3:Subterranean Den", 1, "000200", "1/1", Modifier: "+5"),
            new DropLocationModel("Episode 1", "Hard", "", "Governement Quests Hunter 1‑3:Subterranean Den", 1, "000202", "1/1", Modifier: "+5"),
            new DropLocationModel("Episode 1", "Very Hard", "", "Governement Quests Hunter 1‑3:Subterranean Den", 1, "000204", "1/1", Modifier: "+5"),
            new DropLocationModel("Episode 1", "Ultimate", "", "Governement Quests Hunter 1‑3:Subterranean Den", 1, "000207", "1/1", Modifier: "0/25/0/0/25"),
            new DropLocationModel("Episode 1", "Normal", "", "Governement Quests Ranger 1‑3:Subterranean Den", 2, "000700", "1/1", Modifier: "+5"),
            new DropLocationModel("Episode 1", "Hard", "", "Governement Quests Ranger 1‑3:Subterranean Den", 2, "000702", "1/1", Modifier: "+5"),
            new DropLocationModel("Episode 1", "Very Hard", "", "Governement Quests Ranger 1‑3:Subterranean Den", 2, "000704", "1/1", Modifier: "+5"),
            new DropLocationModel("Episode 1", "Ultimate", "", "Governement Quests Ranger 1‑3:Subterranean Den", 2, "000707", "1/1", Modifier: "0/25/0/0/25"),
            new DropLocationModel("Episode 1", "Normal", "", "Governement Quests Force 1‑3:Subterranean Den", 3, "03020F", "1/1", Modifier: "LV3 disk"),
            new DropLocationModel("Episode 1", "Hard", "", "Governement Quests Force 1‑3:Subterranean Den", 3, "03020F", "1/1", Modifier: "LV8 disk"),
            new DropLocationModel("Episode 1", "Very Hard", "", "Governement Quests Force 1‑3:Subterranean Den", 3, "03020F", "1/1", Modifier: "LV13 disk"),
            new DropLocationModel("Episode 1", "Ultimate", "", "Governement Quests Force 1‑3:Subterranean Den", 3, "03020F", "1/1", Modifier: "LV23 disk"),
            new DropLocationModel("Episode 1", "Normal", "", "Governement Quests Hunter 2‑4:Waterway Shadow", 4, "000302", "1/1", Modifier: "+10"),
            new DropLocationModel("Episode 1", "Hard", "", "Governement Quests Hunter 2‑4:Waterway Shadow", 4, "000303", "1/1", Modifier: "+10"),
            new DropLocationModel("Episode 1", "Very Hard", "", "Governement Quests Hunter 2‑4:Waterway Shadow", 4, "000304", "1/1", Modifier: "+10"),
            new DropLocationModel("Episode 1", "Ultimate", "", "Governement Quests Hunter 2‑4:Waterway Shadow", 4, "000307", "1/1", Modifier: "0/0/25/0/25"),
            new DropLocationModel("Episode 1", "Normal", "", "Governement Quests Ranger 2‑4:Waterway Shadow", 5, "000800", "1/1", Modifier: "+9"),
            new DropLocationModel("Episode 1", "Hard", "", "Governement Quests Ranger 2‑4:Waterway Shadow", 5, "000802", "1/1", Modifier: "+9"),
            new DropLocationModel("Episode 1", "Very Hard", "", "Governement Quests Ranger 2‑4:Waterway Shadow", 5, "000804", "1/1", Modifier: "+9"),
            new DropLocationModel("Episode 1", "Ultimate", "", "Governement Quests Ranger 2‑4:Waterway Shadow", 5, "000807", "1/1", Modifier: "0/0/25/0/25"),
            new DropLocationModel("Episode 1", "Normal", "", "Governement Quests Force 2‑4:Waterway Shadow", 6, "03020B", "1/1", Modifier: "LV4 disk"),
            new DropLocationModel("Episode 1", "Normal", "", "Governement Quests Force 2‑4:Waterway Shadow", 6, "03020D", "1/1", Modifier: "LV4 disk"),
            new DropLocationModel("Episode 1", "Hard", "", "Governement Quests Force 2‑4:Waterway Shadow", 6, "03020B", "1/1", Modifier: "LV9 disk"),
            new DropLocationModel("Episode 1", "Hard", "", "Governement Quests Force 2‑4:Waterway Shadow", 6, "03020D", "1/1", Modifier: "LV9 disk"),
            new DropLocationModel("Episode 1", "Very Hard", "", "Governement Quests Force 2‑4:Waterway Shadow", 6, "03020B", "1/1", Modifier: "LV14 disk"),
            new DropLocationModel("Episode 1", "Very Hard", "", "Governement Quests Force 2‑4:Waterway Shadow", 6, "03020D", "1/1", Modifier: "LV14 disk"),
            new DropLocationModel("Episode 1", "Ultimate", "", "Governement Quests Force 2‑4:Waterway Shadow", 6, "03020B", "1/1", Modifier: "LV24 disk"),
            new DropLocationModel("Episode 1", "Ultimate", "", "Governement Quests Force 2‑4:Waterway Shadow", 6, "03020D", "1/1", Modifier: "LV24 disk"),
            new DropLocationModel("Episode 1", "Normal", "", "Governement Quests Hunter 3‑3:Central Control", 7, "000500", "1/1", Modifier: "+15"),
            new DropLocationModel("Episode 1", "Hard", "", "Governement Quests Hunter 3‑3:Central Control", 7, "000502", "1/1", Modifier: "+15"),
            new DropLocationModel("Episode 1", "Very Hard", "", "Governement Quests Hunter 3‑3:Central Control", 7, "000504", "1/1", Modifier: "+10"),
            new DropLocationModel("Episode 1", "Ultimate", "", "Governement Quests Hunter 3‑3:Central Control", 7, "000507", "1/1", Modifier: "0/0/0/25/25"),
            new DropLocationModel("Episode 1", "Normal", "", "Governement Quests Ranger 3‑3:Central Control", 8, "000900", "1/1", Modifier: "+15"),
            new DropLocationModel("Episode 1", "Hard", "", "Governement Quests Ranger 3‑3:Central Control", 8, "000902", "1/1", Modifier: "+15"),
            new DropLocationModel("Episode 1", "Very Hard", "", "Governement Quests Ranger 3‑3:Central Control", 8, "000904", "1/1", Modifier: "+10"),
            new DropLocationModel("Episode 1", "Ultimate", "", "Governement Quests Ranger 3‑3:Central Control", 8, "000907", "1/1", Modifier: "0/0/0/25/25"),
            new DropLocationModel("Episode 1", "Normal", "", "Governement Quests Force 3‑3:Central Control", 9, "03020A", "1/1", Modifier: "LV5 disk"),
            new DropLocationModel("Episode 1", "Normal", "", "Governement Quests Force 3‑3:Central Control", 9, "03020C", "1/1", Modifier: "LV5 disk"),
            new DropLocationModel("Episode 1", "Hard", "", "Governement Quests Force 3‑3:Central Control", 9, "03020A", "1/1", Modifier: "LV10 disk"),
            new DropLocationModel("Episode 1", "Hard", "", "Governement Quests Force 3‑3:Central Control", 9, "03020C", "1/1", Modifier: "LV10 disk"),
            new DropLocationModel("Episode 1", "Very Hard", "", "Governement Quests Force 3‑3:Central Control", 9, "03020A", "1/1", Modifier: "LV15 disk"),
            new DropLocationModel("Episode 1", "Very Hard", "", "Governement Quests Force 3‑3:Central Control", 9, "03020C", "1/1", Modifier: "LV15 disk"),
            new DropLocationModel("Episode 1", "Ultimate", "", "Governement Quests Force 3‑3:Central Control", 9, "03020A", "1/1", Modifier: "LV25 disk"),
            new DropLocationModel("Episode 1", "Ultimate", "", "Governement Quests Force 3‑3:Central Control", 9, "03020C", "1/1", Modifier: "LV25 disk"),
            new DropLocationModel("Episode 2", "Normal", "", "Governement Quests Hunter 5‑5:Test/VR Temple 5", 10, "000103", "1/1", Modifier: "+10"),
            new DropLocationModel("Episode 2", "Hard", "", "Governement Quests Hunter 5‑5:Test/VR Temple 5", 10, "000104", "1/1", Modifier: "+10"),
            new DropLocationModel("Episode 2", "Very Hard", "", "Governement Quests Hunter 5‑5:Test/VR Temple 5", 10, "000107", "1/1", Modifier: "+10"),
            new DropLocationModel("Episode 2", "Ultimate", "", "Governement Quests Hunter 5‑5:Test/VR Temple 5", 10, "000D00", "1/1", Modifier: "+10 0/0/0/0/50"),
            new DropLocationModel("Episode 2", "Normal", "", "Governement Quests Ranger 5‑5:Test/VR Temple 5", 11, "000103", "1/1", Modifier: "+10"),
            new DropLocationModel("Episode 2", "Hard", "", "Governement Quests Ranger 5‑5:Test/VR Temple 5", 11, "000104", "1/1", Modifier: "+10"),
            new DropLocationModel("Episode 2", "Very Hard", "", "Governement Quests Ranger 5‑5:Test/VR Temple 5", 11, "000107", "1/1", Modifier: "+10"),
            new DropLocationModel("Episode 2", "Ultimate", "", "Governement Quests Ranger 5‑5:Test/VR Temple 5", 11, "000706", "1/1", Modifier: "0/0/0/0/50"),
            new DropLocationModel("Episode 2", "Normal", "", "Governement Quests Force 5‑5:Test/VR Temple 5", 12, "030200", "1/1", Modifier: "LV8 disk"),
            new DropLocationModel("Episode 2", "Hard", "", "Governement Quests Force 5‑5:Test/VR Temple 5", 12, "030201", "1/1", Modifier: "LV13 disk"),
            new DropLocationModel("Episode 2", "Very Hard", "", "Governement Quests Force 5‑5:Test/VR Temple 5", 12, "030202", "1/1", Modifier: "LV16 disk"),
            new DropLocationModel("Episode 2", "Ultimate", "", "Governement Quests Force 5‑5:Test/VR Temple 5", 12, "000E00", "1/1", Modifier: "0/0/0/0/50"),
            new DropLocationModel("Episode 2", "Normal", "", "Governement Quests Hunter 6‑5:Test/Spaceship 5", 13, "000603", "1/1", Modifier: "+15"),
            new DropLocationModel("Episode 2", "Hard", "", "Governement Quests Hunter 6‑5:Test/Spaceship 5", 13, "000604", "1/1", Modifier: "+15"),
            new DropLocationModel("Episode 2", "Very Hard", "", "Governement Quests Hunter 6‑5:Test/Spaceship 5", 13, "000607", "1/1", Modifier: "+15"),
            new DropLocationModel("Episode 2", "Ultimate", "", "Governement Quests Hunter 6‑5:Test/Spaceship 5", 13, "000406", "1/1", Modifier : "0/0/0/0/40"),
            new DropLocationModel("Episode 2", "Normal", "", "Governement Quests Ranger 6‑5:Test/Spaceship 5", 14, "000603", "1/1", Modifier: "+15"),
            new DropLocationModel("Episode 2", "Hard", "", "Governement Quests Ranger 6‑5:Test/Spaceship 5", 14, "000604", "1/1", Modifier: "+15"),
            new DropLocationModel("Episode 2", "Very Hard", "", "Governement Quests Ranger 6‑5:Test/Spaceship 5", 14, "000607", "1/1", Modifier: "+15"),
            new DropLocationModel("Episode 2", "Ultimate", "", "Governement Quests Ranger 6‑5:Test/Spaceship 5", 14, "000906", "1/1", Modifier: "0/0/0/0/40"),
            new DropLocationModel("Episode 2", "Normal", "", "Governement Quests Force 6‑5:Test/Spaceship 5", 15, "030203", "1/1", Modifier: "LV10 disk"),
            new DropLocationModel("Episode 2", "Hard", "", "Governement Quests Force 6‑5:Test/Spaceship 5", 15, "030204", "1/1", Modifier: "LV15 disk"),
            new DropLocationModel("Episode 2", "Very Hard", "", "Governement Quests Force 6‑5:Test/Spaceship 5", 15, "030205", "1/1", Modifier: "LV18 disk"),
            new DropLocationModel("Episode 2", "Ultimate", "", "Governement Quests Force 6‑5:Test/Spaceship 5", 15, "000B05", "1/1", Modifier: "0/0/0/0/40"),
            new DropLocationModel("Episode 2", "Normal", "", "Governement Quests Hunter 7‑5:Isle of Mutants", 16, "000403", "1/1", Modifier: "+20"),
            new DropLocationModel("Episode 2", "Hard", "", "Governement Quests Hunter 7‑5:Isle of Mutants", 16, "000404", "1/1", Modifier: "+10"),
            new DropLocationModel("Episode 2", "Very Hard", "", "Governement Quests Hunter 7‑5:Isle of Mutants", 16, "008900", "1/1", Modifier: "+20"),
            new DropLocationModel("Episode 2", "Ultimate", "", "Governement Quests Hunter 7‑5:Isle of Mutants", 16, "008901", "1/1", Modifier: "0/0/0/0/30"),
            new DropLocationModel("Episode 2", "Normal", "", "Governement Quests Ranger 7‑5:Isle of Mutants", 17, "000403", "1/1", Modifier: "+20"),
            new DropLocationModel("Episode 2", "Hard", "", "Governement Quests Ranger 7‑5:Isle of Mutants", 17, "000404", "1/1", Modifier: "+10"),
            new DropLocationModel("Episode 2", "Very Hard", "", "Governement Quests Ranger 7‑5:Isle of Mutants", 17, "008B00", "1/1", Modifier: "+9"),
            new DropLocationModel("Episode 2", "Ultimate", "", "Governement Quests Ranger 7‑5:Isle of Mutants", 17, "008B01", "1/1", Modifier: "0/0/0/0/30"),
            new DropLocationModel("Episode 2", "Normal", "", "Governement Quests Force 7‑5:Isle of Mutants", 18, "030206", "1/1", Modifier: "LV12 disk"),
            new DropLocationModel("Episode 2", "Hard", "", "Governement Quests Force 7‑5:Isle of Mutants", 18, "030207", "1/1", Modifier: "LV17 disk"),
            new DropLocationModel("Episode 2", "Very Hard", "", "Governement Quests Force 7‑5:Isle of Mutants", 18, "030208", "1/1", Modifier: "LV20 disk"),
            new DropLocationModel("Episode 2", "Ultimate", "", "Governement Quests Force 7‑5:Isle of Mutants", 18, "008C01", "1/1", Modifier: "0/0/0/0/30"),
            new DropLocationModel("Episode 1", "Normal", "", "Say yes to Alicia on Gallon's Plan", 0, "031004", "1/1"),
            new DropLocationModel("Episode 1", "Hard", "", "Say yes to Alicia on Gallon's Plan", 0, "031004", "1/1"),
            new DropLocationModel("Episode 1", "Very Hard", "", "Say yes to Alicia on Gallon's Plan", 0, "031004", "2/1"),
            new DropLocationModel("Episode 1", "Ultimate", "", "Say yes to Alicia on Gallon's Plan", 0, "031004", "2/1")]);

        return dropsLocations.OrderBy(OrderByLocations);
    }

    private static DropLocationModel CreateDropLocationModel(NamedObject<EpisodeModel> episode, NamedObject<DifficultyModel> difficulty, NamedObject<SectionIdModel> sectionId, KeyValuePair<string, RareSpecificationModel[]> dropLocation, RareSpecificationModel itemProbability, Dictionary<int, Dictionary<string, (int Index, string Name, string UltimateName)>> locations, ConfigModel config)
    {
        int episodeNumber = (int)char.GetNumericValue(episode.Name[^1]);
        return new DropLocationModel(episode.Name, difficulty.Name, sectionId.Name,
            GetLocationName(locations[episodeNumber], dropLocation.Key, difficulty.Name),
            OrderByLocations(locations[episodeNumber], dropLocation.Key),
            itemProbability.ItemDescription.Replace("0x", ""), config.ApplyDropRateMultiplier(itemProbability.Probability), true);
    }

    private static string GetLocationName(Dictionary<string, (int Index, string Name, string UltimateName)> locations, string locationId, string difficulty)
        => difficulty == "Ultimate" ? locations[locationId].UltimateName : locations[locationId].Name;

    private static int OrderByLocations(Dictionary<string, (int Index, string Name, string UltimateName)> locations, string locationId)
    {
        // Locations can have duplicate names, so we take the order of the first one we find.
        return locations.First(l2 => l2.Value.Name == locations[locationId].Name).Value.Index;
    }

    private static int OrderByLocations(DropLocationModel dropLocation)
    {
        int episodeNumber = string.IsNullOrEmpty(dropLocation.EpisodeName) ? 0 : (int)char.GetNumericValue(dropLocation.EpisodeName[^1]);
        int DifficultyNameToOrder(string name) => name switch
        {
            "Normal" or "" => 0,
            "Hard" => 1,
            "Very Hard" => 2,
            "Ultimate" => 3,
            _ => throw new NotSupportedException()
        };
        int SectionIdNameToId(string name) => name switch
        {
            "Viridia" or "" => 0,
            "Greenill" => 1,
            "Skyly" => 2,
            "Bluefull" => 3,
            "Purplenum" => 4,
            "Pinkal" => 5,
            "Redria" => 6,
            "Oran" => 7,
            "Yellowboze" => 8,
            "Whitill" => 9,
            _ => throw new NotSupportedException()
        };
        return DifficultyNameToOrder(dropLocation.DifficultyName) * 100_000 + episodeNumber * 10000 + dropLocation.Order * 10 + SectionIdNameToId(dropLocation.SectionId);
    }
}

public record DropLocationModel(string EpisodeName, string DifficultyName, string SectionId, string LocationName, int Order, string ItemIdentifier, string Probability, bool IsRareDrop = false, string? Modifier = null)
{
    public int EpisodeNumber => (int)char.GetNumericValue(EpisodeName[^1]);
}

public class SectionIdComparer : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x == null || y == null || x == y) return 0;
        return Array.IndexOf(s_orderedSectionsId, x).CompareTo(Array.IndexOf(s_orderedSectionsId, y));
    }

    private static readonly string[] s_orderedSectionsId = ["Viridia", "Greenill", "Skyly", "Bluefull", "Purplenum", "Pinkal", "Redria", "Oran", "Yellowboze", "Whitill"];
}
