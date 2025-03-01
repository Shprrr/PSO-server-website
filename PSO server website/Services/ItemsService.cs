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

    public IEnumerable<ItemModel> GetItems() => _items.Select(i => GetItem(i.Key)!);

    public ItemModel? GetItem(string itemIdentifier)
    {
        if (!_items.TryGetValue(itemIdentifier, out ItemNameModel? item)) return null;
        return GetItemType(itemIdentifier) switch
        {
            ItemType.Weapon => new WeaponItemModel(item.Identifier, item.Name, GetIcon(itemIdentifier),
                _itemPMT.Weapons[itemIdentifier].Id, _itemPMT.Weapons[itemIdentifier].Type,
                _itemPMT.Weapons[itemIdentifier].Skin, _itemPMT.Weapons[itemIdentifier].TeamPoints,
                (Special)_itemPMT.Weapons[itemIdentifier].Special, GetSpecialName(_itemPMT.Weapons[itemIdentifier].Special)),
            _ => new ItemModel(item.Identifier, item.Name, GetItemType(itemIdentifier), GetIcon(itemIdentifier)),
        };
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

    private static readonly string[] s_specialNames = ["None", "[1]Draw", "[2]Drain", "[3]Fill", "[4]Gush", "[1]Heart", "[2]Mind", "[3]Soul", "[4]Geist", "[1]Master's", "[2]Lord's", "[3]King's", "Charge", "Spirit", "Berserk", "[1]Ice", "[2]Frost", "[3]Freeze", "[4]Blizzard", "[1]Bind", "[2]Hold", "[3]Seize", "[4]Arrest", "[1]Heat", "[2]Fire", "[3]Flame", "[4]Burning", "[1]Shock", "[2]Thunder", "[3]Storm", "[4]Tempest", "[1]Dim", "[2]Shadow", "[3]Dark", "[4]Hell", "[1]Panic", "[2]Riot", "[3]Havoc", "[4]Chaos", "[1]Devil's", "[2]Demon's"];
    public static string GetSpecialName(int special)
    {
        return s_specialNames[special];
    }
}

public record ItemModel(string ItemIdentifier, string ItemName, ItemType ItemType, ItemIcon Icon)
{
    public WeaponItemModel AsWeapon => ItemType == ItemType.Weapon ? (WeaponItemModel)this : throw new InvalidCastException();
}

public record WeaponItemModel(string ItemIdentifier, string ItemName, ItemIcon Icon, uint ItemId, ushort Type, ushort Skin, uint TeamPoints, Special Special, string SpecialName)
    : ItemModel(ItemIdentifier, ItemName, ItemType.Weapon, Icon);

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

public enum Special
{
    None,
    Draw,
    Drain,
    Fill,
    Gush,
    Heart,
    Mind,
    Soul,
    Geist,
    Master,
    Lord,
    King,
    Charge,
    Spirit,
    Berserk,
    Ice,
    Frost,
    Freeze,
    Blizzard,
    Bind,
    Hold,
    Seize,
    Arrest,
    Heat,
    Fire,
    Flame,
    Burning,
    Shock,
    Thunder,
    Storm,
    Tempest,
    Dim,
    Shadow,
    Dark,
    Hell,
    Panic,
    Riot,
    Havoc,
    Chaos,
    Devil,
    Demon
}
