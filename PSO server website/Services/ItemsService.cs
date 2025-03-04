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
        ItemType itemType = GetItemType(itemIdentifier);
        return itemType switch
        {
            ItemType.Weapon => new WeaponItemModel(item.Identifier, item.Name, GetIcon(itemIdentifier),
                _itemPMT.Weapons[itemIdentifier].Id, _itemPMT.Weapons[itemIdentifier].Type,
                _itemPMT.Weapons[itemIdentifier].Skin, _itemPMT.Weapons[itemIdentifier].TeamPoints,
                GetClassFlag(_itemPMT.Weapons[itemIdentifier].ClassFlags), _itemPMT.Weapons[itemIdentifier].ATPRequired,
                _itemPMT.Weapons[itemIdentifier].MSTRequired, _itemPMT.Weapons[itemIdentifier].ATARequired,
                _itemPMT.Weapons[itemIdentifier].ATPMin, _itemPMT.Weapons[itemIdentifier].ATPMax,
                _itemPMT.Weapons[itemIdentifier].MST, _itemPMT.Weapons[itemIdentifier].ATA,
                _itemPMT.Weapons[itemIdentifier].MaxGrind, _itemPMT.Weapons[itemIdentifier].Photon,
                (Special)_itemPMT.Weapons[itemIdentifier].Special, GetSpecialName(_itemPMT.Weapons[itemIdentifier].Special),
                _itemPMT.Weapons[itemIdentifier].Projectile, _itemPMT.Weapons[itemIdentifier].ComboType),

            ItemType.Armor or ItemType.Shield => new ArmorItemModel(item.Identifier, item.Name,
                itemType, GetIcon(itemIdentifier), _itemPMT.Armors[itemIdentifier].Id,
                _itemPMT.Armors[itemIdentifier].Type, _itemPMT.Armors[itemIdentifier].Skin,
                _itemPMT.Armors[itemIdentifier].TeamPoints, GetClassFlag(_itemPMT.Armors[itemIdentifier].ClassFlags),
                _itemPMT.Armors[itemIdentifier].RequiredLevel + 1, _itemPMT.Armors[itemIdentifier].DFP,
                _itemPMT.Armors[itemIdentifier].DFPRange, _itemPMT.Armors[itemIdentifier].EVP,
                _itemPMT.Armors[itemIdentifier].EVPRange, _itemPMT.Armors[itemIdentifier].EFR,
                _itemPMT.Armors[itemIdentifier].ETH, _itemPMT.Armors[itemIdentifier].EIC,
                _itemPMT.Armors[itemIdentifier].EDK, _itemPMT.Armors[itemIdentifier].ELT,
                _itemPMT.Armors[itemIdentifier].BlockParticle, _itemPMT.Armors[itemIdentifier].BlockEffect),

            ItemType.Unit => new UnitItemModel(item.Identifier, item.Name, GetIcon(itemIdentifier),
                _itemPMT.Units[itemIdentifier].Id, _itemPMT.Units[itemIdentifier].Type,
                _itemPMT.Units[itemIdentifier].Skin, _itemPMT.Units[itemIdentifier].TeamPoints,
                GetClassFlag(_itemPMT.Units[itemIdentifier].ClassFlags), _itemPMT.Units[itemIdentifier].ModifierAmount),

            _ => new ItemModel(item.Identifier, item.Name, itemType, GetIcon(itemIdentifier)),
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

    public static ClassFlag GetClassFlag(ClassFeatureFlag classFeatureFlags)
    {
        ClassFlag result = 0;
        if (classFeatureFlags.HasFlag(ItemPMTRepository.ClassFeatureFlagsByClass[0])) result |= ClassFlag.HUmar;
        if (classFeatureFlags.HasFlag(ItemPMTRepository.ClassFeatureFlagsByClass[1])) result |= ClassFlag.HUnewearl;
        if (classFeatureFlags.HasFlag(ItemPMTRepository.ClassFeatureFlagsByClass[2])) result |= ClassFlag.HUcast;
        if (classFeatureFlags.HasFlag(ItemPMTRepository.ClassFeatureFlagsByClass[3])) result |= ClassFlag.HUcaseal;
        if (classFeatureFlags.HasFlag(ItemPMTRepository.ClassFeatureFlagsByClass[4])) result |= ClassFlag.RAmar;
        if (classFeatureFlags.HasFlag(ItemPMTRepository.ClassFeatureFlagsByClass[5])) result |= ClassFlag.RAmarl;
        if (classFeatureFlags.HasFlag(ItemPMTRepository.ClassFeatureFlagsByClass[6])) result |= ClassFlag.RAcast;
        if (classFeatureFlags.HasFlag(ItemPMTRepository.ClassFeatureFlagsByClass[7])) result |= ClassFlag.RAcaseal;
        if (classFeatureFlags.HasFlag(ItemPMTRepository.ClassFeatureFlagsByClass[8])) result |= ClassFlag.FOmar;
        if (classFeatureFlags.HasFlag(ItemPMTRepository.ClassFeatureFlagsByClass[9])) result |= ClassFlag.FOmarl;
        if (classFeatureFlags.HasFlag(ItemPMTRepository.ClassFeatureFlagsByClass[10])) result |= ClassFlag.FOnewm;
        if (classFeatureFlags.HasFlag(ItemPMTRepository.ClassFeatureFlagsByClass[11])) result |= ClassFlag.FOnewearl;
        return result;
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
    public ArmorItemModel AsArmor => ItemType is ItemType.Armor or ItemType.Shield ? (ArmorItemModel)this : throw new InvalidCastException();
    public UnitItemModel AsUnit => ItemType == ItemType.Unit ? (UnitItemModel)this : throw new InvalidCastException();
}

public record WeaponItemModel(string ItemIdentifier, string ItemName, ItemIcon Icon, uint ItemId, ushort Type,
    ushort Skin, uint TeamPoints, ClassFlag EquipableClass, ushort ATPRequired, ushort MSTRequired, ushort ATARequired,
    ushort ATPMin, ushort ATPMax, ushort MST, ushort ATA, byte MaxGrind, byte PhotonId, Special Special, string SpecialName, byte ProjectileId, byte ComboTypeFlag)
    : ItemModel(ItemIdentifier, ItemName, ItemType.Weapon, Icon);

public record ArmorItemModel(string ItemIdentifier, string ItemName, ItemType ItemType, ItemIcon Icon, uint ItemId, ushort Type,
    ushort Skin, uint TeamPoints, ClassFlag EquipableClass, int RequiredLevel, ushort DFP, byte DFPRange, ushort EVP,
    byte EVPRange, byte EFR, byte ETH, byte EIC, byte EDK, byte ELT, byte BlockParticle, byte BlockEffect)
    : ItemModel(ItemIdentifier, ItemName, ItemType, Icon)
{
    public int DFPMax => DFP + DFPRange;
    public int EVPMax => EVP + EVPRange;
}

public record UnitItemModel(string ItemIdentifier, string ItemName, ItemIcon Icon, uint ItemId, ushort Type,
    ushort Skin, uint TeamPoints, ClassFlag EquipableClass, short ModifierAmount)
    : ItemModel(ItemIdentifier, ItemName, ItemType.Unit, Icon)
{
    public bool CanHaveModifier => ItemId switch
    {
        < 908 and not (896 or 900 or 904) => true,
        _ => false
    };
}

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

[Flags]
public enum ClassFlag
{
    HUmar = 0x01,
    HUnewearl = 0x02,
    HUcast = 0x04,
    HUcaseal = 0x08,
    RAmar = 0x10,
    RAmarl = 0x20,
    RAcast = 0x40,
    RAcaseal = 0x80,
    FOmar = 0x100,
    FOmarl = 0x200,
    FOnewm = 0x400,
    FOnewearl = 0x800
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
