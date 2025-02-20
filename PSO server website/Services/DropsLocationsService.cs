using PSOServerWebsite.Repositories;

namespace PSOServerWebsite.Services;

public class DropsLocationsService(RareDropsRepository rareDropsRepository, LocationsRepository locationsRepository, ConfigurationRepository configurationRepository)
{
    public async Task<IEnumerable<DropLocationModel>> GetDropsLocationsAsync()
    {
        RareDropModel rareDrops = await rareDropsRepository.GetRareDropsAsync();
        var locations = (await locationsRepository.GetLocationsAsync())
            .ToDictionary(l => l.Episode, l => l.Locations.Index()
                .ToDictionary(l => l.Item.Id, l => (l.Index, l.Item.Name, UltimateName: string.IsNullOrEmpty(l.Item.UltimateName) ? l.Item.Name : l.Item.UltimateName)));
        ConfigModel config = await configurationRepository.GetConfigAsync();

        return rareDrops.Normal.Episodes()
            .SelectMany(episode => episode.Value.Difficulties()
            .SelectMany(difficulty => difficulty.Value.SectionsId()
            .SelectMany(sectionId => sectionId.Value.SelectMany(dl => dl.Value,
                (dropLocation, itemProbability) => CreateDropLocationModel(episode, difficulty, sectionId, dropLocation, itemProbability, locations, config)))));
    }

    private static DropLocationModel CreateDropLocationModel(NamedObject<EpisodeModel> episode, NamedObject<DifficultyModel> difficulty, NamedObject<SectionIdModel> sectionId, KeyValuePair<string, RareSpecificationModel[]> dropLocation, RareSpecificationModel itemProbability, Dictionary<int, Dictionary<string, (int Index, string Name, string UltimateName)>> locations, ConfigModel config)
    {
        int episodeNumber = (int)char.GetNumericValue(episode.Name[^1]);
        return new DropLocationModel(episode.Name, difficulty.Name, sectionId.Name,
            GetLocationName(locations[episodeNumber], dropLocation.Key, difficulty.Name),
            OrderByLocations(locations[episodeNumber], dropLocation.Key),
            itemProbability.ItemDescription, config.ApplyDropRateMultiplier(itemProbability.Probability));
    }

    private static string GetLocationName(Dictionary<string, (int Index, string Name, string UltimateName)> locations, string locationId, string difficulty)
        => difficulty == "Ultimate" ? locations[locationId].UltimateName : locations[locationId].Name;

    private static int OrderByLocations(Dictionary<string, (int Index, string Name, string UltimateName)> locations, string locationId)
    {
        // Locations can have duplicate names, so we take the order of the first one we find.
        return locations.First(l2 => l2.Value.Name == locations[locationId].Name).Value.Index;
    }
}

public record DropLocationModel(string EpisodeName, string DifficultyName, string SectionId, string LocationName, int Order, string ItemIdentifier, string Probability)
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
