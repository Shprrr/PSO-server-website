using PSOServerWebsite.Repositories;

namespace PSOServerWebsite.Services;

public class ItemsService(ItemsRepository itemsRepository, ItemPMTRepository itemPMTRepository)
{
    private Dictionary<string, ItemNameModel> _items = [];
    private ItemPMTModel _itemPMT = default!;

    public async Task LoadDataAsync()
    {
        _items = (await itemsRepository.GetItemsAsync()).ToDictionary(i => i.Identifier);
        _itemPMT = await itemPMTRepository.GetItemsAsync();
    }

    public IEnumerable<ItemModel> GetItems() => _items.Select(i => new ItemModel(i.Key, i.Value.Name, GetItemType(i.Key), GetIcon(i.Key)));

    public ItemModel? GetItem(string itemIdentifier)
    {
        if (!_items.TryGetValue(itemIdentifier, out ItemNameModel? item)) return null;
        return new ItemModel(item.Identifier, item.Name, GetItemType(itemIdentifier), GetIcon(itemIdentifier));
    }

    public static ItemType GetItemType(string itemIdentifier) => itemIdentifier switch
    {
        { } when itemIdentifier.StartsWith("00") => ItemType.Weapon,
        { } when itemIdentifier.StartsWith("0101") => ItemType.Armor,
        { } when itemIdentifier.StartsWith("0102") => ItemType.Shield,
        { } when itemIdentifier.StartsWith("0103") => ItemType.Unit,
        { } when itemIdentifier.StartsWith("02") => ItemType.Mag,
        { } when itemIdentifier.StartsWith("03") => ItemType.Tool,
        _ => throw new NotSupportedException()
    };

    private static readonly Dictionary<string, ItemIcon> s_iconNames = new()
    {
        { "0101", ItemIcon.Armor },
        { "0102", ItemIcon.Shield },
        { "0103", ItemIcon.Unit },
        { "02", ItemIcon.Mag },
        { "0300", ItemIcon.Mate },
        { "0301", ItemIcon.Fluid },
        { "0302", ItemIcon.Disk },
        { "03", ItemIcon.Tool }
    };
    public ItemIcon GetIcon(string itemIdentifier)
    {
        // For weapons, check the attack animation to determine the weapon type.
        if (itemIdentifier.StartsWith("00"))
        {
            int attackOffset = int.Parse(itemIdentifier.Substring(0, 4), System.Globalization.NumberStyles.HexNumber);
            return _itemPMT.AttackAnimations[attackOffset] switch
            {
                0 or 1 or 2 or 3 or 4 or 5 or 13 or 14 or 15 or 16 => ItemIcon.Sword,
                6 or 7 or 8 or 9 or 17 => ItemIcon.Gun,
                10 or 11 or 12 or 18 => ItemIcon.Cane,
                _ => throw new NotSupportedException(),
            };
        }

        return s_iconNames.Where(i => itemIdentifier.StartsWith(i.Key)).First().Value;
    }
}

public record ItemModel(string ItemIdentifier, string ItemName, ItemType ItemType, ItemIcon Icon);

public enum ItemType
{
    Weapon,
    Armor,
    Shield,
    Unit,
    Mag,
    Tool
}

public enum ItemIcon
{
    Sword,
    Gun,
    Cane,
    Armor,
    Shield,
    Unit,
    Mag,
    Tool,
    Mate,
    Fluid,
    Disk
}
