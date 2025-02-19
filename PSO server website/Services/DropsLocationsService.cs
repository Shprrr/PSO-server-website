using PSOServerWebsite.Repositories;

namespace PSOServerWebsite.Services;

public class DropsLocationsService(RareDropsRepository rareDropsRepository, LocationsRepository locationsRepository, ConfigurationRepository configurationRepository)
{
    public async Task<IEnumerable<DropLocationModel>> GetDropsLocationsAsync()
    {
        RareDropModel rareDrops = await rareDropsRepository.GetRareDropsAsync();
        var locations = (await locationsRepository.GetLocationsAsync()).ToDictionary(l => l.Episode, l => l.Locations);
        ConfigModel config = await configurationRepository.GetConfigAsync();

        return rareDrops.Normal.Episodes()
            .SelectMany(episode => episode.Value.Difficulties()
            .SelectMany(difficulty => difficulty.Value.SectionsId()
            .SelectMany(sectionId => sectionId.Value.SelectMany(dl => dl.Value,
                (dropLocation, itemProbability) => new DropLocationModel(episode.Name, difficulty.Name, sectionId.Name,
                    GetLocationName(locations, dropLocation.Key, episode.Name, difficulty.Name),
                    OrderByLocations(locations, dropLocation.Key, episode.Name),
                    itemProbability.ItemDescription, config.ApplyDropRateMultiplier(itemProbability.Probability))))));
    }

    private static string GetLocationName(Dictionary<int, LocationWhereModel[]> locations, string locationId, string episode, string difficulty)
    {
        int episodeNumber = (int)char.GetNumericValue(episode[^1]);
        LocationWhereModel? location = locations[episodeNumber].FirstOrDefault(l => l.Id == locationId);
        if (location == null) return locationId;
        if (difficulty == "Ultimate" && !string.IsNullOrEmpty(location.UltimateName))
            return location.UltimateName;
        return location.Name;
    }

    private static int OrderByLocations(Dictionary<int, LocationWhereModel[]> locations, string locationId, string episode)
    {
        int episodeNumber = (int)char.GetNumericValue(episode[^1]);
        // Locations can have duplicate names, so we take the order of the first one we find.
        return locations[episodeNumber].Index().First(l2 => l2.Item.Name == locations[episodeNumber].First(l1 => l1.Id == locationId).Name).Index;
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
