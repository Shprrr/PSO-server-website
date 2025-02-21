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

        return dropsLocations;
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
}

public record DropLocationModel(string EpisodeName, string DifficultyName, string SectionId, string LocationName, int Order, string ItemIdentifier, string Probability, bool IsRareDrop = false)
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
