using System.Runtime.InteropServices;

namespace PSOServerWebsite.Services;

public class ItemPMTService(HttpClient http)
{
    public static readonly ClassFlag[] ClassFlagsByClass = [
        ClassFlag.HUNTER | ClassFlag.HUMAN | ClassFlag.MALE, // HUmar
        ClassFlag.HUNTER | ClassFlag.NEWMAN | ClassFlag.FEMALE, // HUnewearl
        ClassFlag.HUNTER | ClassFlag.ANDROID | ClassFlag.MALE, // HUcast
        ClassFlag.HUNTER | ClassFlag.ANDROID | ClassFlag.FEMALE, // HUcaseal
        ClassFlag.RANGER | ClassFlag.HUMAN | ClassFlag.MALE, // RAmar
        ClassFlag.RANGER | ClassFlag.HUMAN | ClassFlag.FEMALE, // RAmarl
        ClassFlag.RANGER | ClassFlag.ANDROID | ClassFlag.MALE, // RAcast
        ClassFlag.RANGER | ClassFlag.ANDROID | ClassFlag.FEMALE, // RAcaseal
        ClassFlag.FORCE | ClassFlag.HUMAN | ClassFlag.MALE, // FOmar
        ClassFlag.FORCE | ClassFlag.HUMAN | ClassFlag.FEMALE, // FOmarl
        ClassFlag.FORCE | ClassFlag.NEWMAN | ClassFlag.MALE, // FOnewm
        ClassFlag.FORCE | ClassFlag.NEWMAN | ClassFlag.FEMALE, // FOnewearl
    ];

    // Offset from end of ItemPMT.bin file to main pointer table.
    private const int OffsetStart = 16;

    // Offsets for main table, in bytes.
    private static readonly Dictionary<string, int> s_offset = new(){
        { "weapon",            0 },
        { "armor",             4 },
        { "unit",              8 },
        { "tool",              12 },
        { "mag",               16 },
        { "AA",                20 },
        { "photon-color",      24 },
        { "weapon-range",      28 },
        { "weapon-sales-div",  32 },
        { "sales-div",         36 },
        { "mag-feed",          40 },
        { "star-value",        44 },
        { "special",           48 },
        { "weapon-SFX",        52 },
        { "stat-boost",        56 },
        { "shield-sfx",        60 },
        { "tech-use",          64 },
        { "combination",       68 },
        //{ "unknown-table",     72 },
        { "tech-boost",        76 },
        { "unwrap",            80 },
        { "unsealable",        84 },
        { "ranged-special",    88 },
    };
    private const int WeaponClassesCount = 0xED;
    private const int WeaponSize = 0x2C;
    private const int ArmorClassesCount = 2;
    private const int ArmorClassesIdentifierOffset = 0x101;
    private const int ArmorSize = 0x20;
    private const int UnitClassesCount = 1;
    private const int UnitClassesIdentifierOffset = 0x103;
    private const int UnitSize = 0x14;
    private const int ToolClassesCount = 0x1A;
    private const int ToolClassesIdentifierOffset = 0x300;
    private const int ToolSize = 0x18;
    private const int SpecialCount = 0x29;
    private const int SpecialSize = 0x4;
    private const int StatBoostCount = 52;
    private const int StatBoostSize = 0x6;

    public async Task<ItemPMTModel> GetItemsAsync()
    {
        byte[] fileBytes = await http.GetByteArrayAsync("data/ItemPMT-bb-v4.bin");

        // Seek to start offset
        int offset = fileBytes.Length - OffsetStart;

        // Read the address
        int startAddr = (int)BitConverter.ToInt64(fileBytes, offset);

        // Go to the bin table based on the offset in the lookup table
        offset = startAddr + s_offset["weapon"];

        int tableAddr = BitConverter.ToInt32(fileBytes, offset);

        Dictionary<string, WeaponModel> weapons = new(WeaponClassesCount);
        for (int i = 0; i < WeaponClassesCount; i++)
        {
            int subtableCount = BitConverter.ToInt32(fileBytes, tableAddr + sizeof(int) * i * 2);
            int subtableOffset = BitConverter.ToInt32(fileBytes, tableAddr + sizeof(int) * (i * 2 + 1));

            for (int j = 0; j < subtableCount; j++)
            {
                weapons.Add($"{i:X4}{j:X2}", BitConverterExtensions.ToWeaponModel(fileBytes, subtableOffset + WeaponSize * j));
            }
        }

        offset = startAddr + s_offset["armor"];
        tableAddr = BitConverter.ToInt32(fileBytes, offset);

        Dictionary<string, ArmorModel> armors = new(ArmorClassesCount);
        for (int i = 0; i < ArmorClassesCount; i++)
        {
            int subtableCount = BitConverter.ToInt32(fileBytes, tableAddr + sizeof(int) * i * 2);
            int subtableOffset = BitConverter.ToInt32(fileBytes, tableAddr + sizeof(int) * (i * 2 + 1));

            for (int j = 0; j < subtableCount; j++)
            {
                armors.Add($"{i + ArmorClassesIdentifierOffset:X4}{j:X2}", BitConverterExtensions.ToArmorModel(fileBytes, subtableOffset + ArmorSize * j));
            }
        }

        offset = startAddr + s_offset["unit"];
        tableAddr = BitConverter.ToInt32(fileBytes, offset);

        Dictionary<string, UnitModel> units = new(UnitClassesCount);
        for (int i = 0; i < UnitClassesCount; i++)
        {
            int subtableCount = BitConverter.ToInt32(fileBytes, tableAddr + sizeof(int) * i * 2);
            int subtableOffset = BitConverter.ToInt32(fileBytes, tableAddr + sizeof(int) * (i * 2 + 1));

            for (int j = 0; j < subtableCount; j++)
            {
                units.Add($"{i + UnitClassesIdentifierOffset:X4}{j:X2}", BitConverterExtensions.ToUnitModel(fileBytes, subtableOffset + UnitSize * j));
            }
        }

        offset = startAddr + s_offset["tool"];
        tableAddr = BitConverter.ToInt32(fileBytes, offset);

        Dictionary<string, ToolModel> tools = new(ToolClassesCount);
        for (int i = 0; i < ToolClassesCount; i++)
        {
            int subtableCount = BitConverter.ToInt32(fileBytes, tableAddr + sizeof(int) * i * 2);
            int subtableOffset = BitConverter.ToInt32(fileBytes, tableAddr + sizeof(int) * (i * 2 + 1));

            for (int j = 0; j < subtableCount; j++)
            {
                tools.Add($"{i + ToolClassesIdentifierOffset:X4}{j:X2}", BitConverterExtensions.ToToolModel(fileBytes, subtableOffset + ToolSize * j));
            }
        }

        offset = startAddr + s_offset["special"];
        tableAddr = BitConverter.ToInt32(fileBytes, offset);

        List<SpecialModel> specials = [];
        for (int j = 0; j < SpecialCount; j++)
        {
            specials.Add(BitConverterExtensions.ToSpecialModel(fileBytes, tableAddr + SpecialSize * j));
        }

        offset = startAddr + s_offset["stat-boost"];
        tableAddr = BitConverter.ToInt32(fileBytes, offset);

        List<StatBoostModel> statBoosts = [];
        for (int j = 0; j < StatBoostCount; j++)
        {
            statBoosts.Add(BitConverterExtensions.ToStatBoostModel(fileBytes, tableAddr + StatBoostSize * j));
        }

        return new ItemPMTModel { Weapons = weapons, Armors = armors, Units = units, Tools = tools, Specials = [.. specials], StatBoosts = [.. statBoosts] };
    }
}

public class ItemPMTModel
{
    public ItemCountModel ItemCount { get; set; } = new();
    public Dictionary<string, WeaponModel> Weapons { get; set; } = [];
    public Dictionary<string, ArmorModel> Armors { get; set; } = [];
    public Dictionary<string, UnitModel> Units { get; set; } = [];
    public Dictionary<string, ToolModel> Tools { get; set; } = [];
    public SpecialModel[] Specials { get; set; } = [];
    public StatBoostModel[] StatBoosts { get; set; } = [];
}

public class ItemCountModel
{
    public uint WeaponCount { get; set; }
    public uint ArmorCount { get; set; }
    public uint ShieldCount { get; set; }
    public uint UnitCount { get; set; }
    public uint ItemCount { get; set; }
    public uint MagCount { get; set; }
    public uint StarValueCount { get; set; }
    public uint SpecialCount { get; set; }
    public uint StatBoostCount { get; set; }
    public uint CombinationCount { get; set; }
    public uint TechBoostCount { get; set; }
    public uint UnwrapCount { get; set; }
    public uint XmasItemCount { get; set; }
    public uint EasterItemCount { get; set; }
    public uint HalloweenItemCcount { get; set; }
    public uint UnsealableCount { get; set; }
}

public class ItemBaseModel
{
    public uint Id { get; set; }
    public ushort Type { get; set; }
    public ushort Skin { get; set; }
    public uint TeamPoints { get; set; }
}

public class WeaponModel : ItemBaseModel
{
    public ushort ClassFlagsRaw { get; set; }
    public ClassFlag ClassFlags => (ClassFlag)ClassFlagsRaw;
    public ushort ATPMin { get; set; }
    public ushort ATPMax { get; set; }
    public ushort ATPRequired { get; set; }
    public ushort MSTRequired { get; set; }
    public ushort ATARequired { get; set; }
    public ushort MST { get; set; }
    public byte MaxGrind { get; set; }
    public byte Photon { get; set; }
    public byte Special { get; set; }
    public byte ATA { get; set; }
    public byte StatBoost { get; set; }
    public byte Projectile { get; set; }
    public sbyte Trail1X { get; set; }
    public sbyte Trail1Y { get; set; }
    public sbyte Trail2X { get; set; }
    public sbyte Trail2Y { get; set; }
    public sbyte Color { get; set; }
    public byte Unknown1 { get; set; }
    public byte Unknown2 { get; set; }
    public byte Unknown3 { get; set; }
    public byte Unknown4 { get; set; }
    public byte Unknown5 { get; set; }
    public byte TechBoost { get; set; }
    public byte ComboType { get; set; }
}

public class ArmorModel : ItemBaseModel
{
    public ushort DFP { get; set; }
    public ushort EVP { get; set; }
    public byte BlockParticle { get; set; }
    public byte BlockEffect { get; set; }
    public ushort ClassFlagsRaw { get; set; }
    public ClassFlag ClassFlags => (ClassFlag)ClassFlagsRaw;
    public byte RequiredLevel { get; set; }
    public byte EFR { get; set; }
    public byte ETH { get; set; }
    public byte EIC { get; set; }
    public byte EDK { get; set; }
    public byte ELT { get; set; }
    public byte DFPRange { get; set; }
    public byte EVPRange { get; set; }
}

public class UnitModel : ItemBaseModel
{
    public ushort Stat { get; set; }
    public ushort StatAmount { get; set; }
    public short ModifierAmount { get; set; }
}

public class ToolModel : ItemBaseModel
{
    public ushort Amount { get; set; }
    public ushort Technique { get; set; }
    public int Cost { get; set; }
    public uint ItemFlag { get; set; }
}

public class SpecialModel
{
    public ushort Type { get; set; }
    public ushort Amount { get; set; }
}

public class StatBoostModel
{
    public byte Stat1 { get; set; }
    public byte Stat2 { get; set; }
    public ushort Amount1 { get; set; }
    public ushort Amount2 { get; set; }
}

[Flags]
public enum ClassFlag
{
    HUNTER = 0x01,
    RANGER = 0x02,
    FORCE = 0x04,
    HUMAN = 0x08,
    ANDROID = 0x10,
    NEWMAN = 0x20,
    MALE = 0x40,
    FEMALE = 0x80,
};

public static partial class BitConverterExtensions
{
    private static void ToItemBaseModel(byte[] array, ref int offset, ref ItemBaseModel itemBase)
    {
        itemBase.Id = BitConverter.ToUInt32(array, offset);
        offset += Marshal.SizeOf(itemBase.Id);
        itemBase.Type = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(itemBase.Type);
        itemBase.Skin = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(itemBase.Skin);
        itemBase.TeamPoints = BitConverter.ToUInt32(array, offset);
        offset += Marshal.SizeOf(itemBase.TeamPoints);
    }

    public static WeaponModel ToWeaponModel(byte[] array, int offset)
    {
        WeaponModel item = new();
        ItemBaseModel itemBase = item;
        ToItemBaseModel(array, ref offset, ref itemBase);

        item.ClassFlagsRaw = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(item.ClassFlagsRaw);
        item.ATPMin = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(item.ATPMin);
        item.ATPMax = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(item.ATPMax);
        item.ATPRequired = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(item.ATPRequired);
        item.MSTRequired = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(item.MSTRequired);
        item.ATARequired = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(item.ATARequired);
        item.MST = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(item.MST);
        item.MaxGrind = array[offset];
        offset += Marshal.SizeOf(item.MaxGrind);
        item.Photon = array[offset];
        offset += Marshal.SizeOf(item.Photon);
        item.Special = array[offset];
        offset += Marshal.SizeOf(item.Special);
        item.ATA = array[offset];
        offset += Marshal.SizeOf(item.ATA);
        item.StatBoost = array[offset];
        offset += Marshal.SizeOf(item.StatBoost);
        item.Projectile = array[offset];
        offset += Marshal.SizeOf(item.Projectile);
        item.Trail1X = (sbyte)array[offset];
        offset += Marshal.SizeOf(item.Trail1X);
        item.Trail1Y = (sbyte)array[offset];
        offset += Marshal.SizeOf(item.Trail1Y);
        item.Trail2X = (sbyte)array[offset];
        offset += Marshal.SizeOf(item.Trail2X);
        item.Trail2Y = (sbyte)array[offset];
        offset += Marshal.SizeOf(item.Trail2Y);
        item.Color = (sbyte)array[offset];
        offset += Marshal.SizeOf(item.Color);
        item.Unknown1 = array[offset];
        offset += Marshal.SizeOf(item.Unknown1);
        item.Unknown2 = array[offset];
        offset += Marshal.SizeOf(item.Unknown2);
        item.Unknown3 = array[offset];
        offset += Marshal.SizeOf(item.Unknown3);
        item.Unknown4 = array[offset];
        offset += Marshal.SizeOf(item.Unknown4);
        item.Unknown5 = array[offset];
        offset += Marshal.SizeOf(item.Unknown5);
        item.TechBoost = array[offset];
        offset += Marshal.SizeOf(item.TechBoost);
        item.ComboType = array[offset];
        offset += Marshal.SizeOf(item.ComboType);
        return item;
    }

    public static ArmorModel ToArmorModel(byte[] array, int offset)
    {
        ArmorModel item = new();
        ItemBaseModel itemBase = item;
        ToItemBaseModel(array, ref offset, ref itemBase);

        item.DFP = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(item.DFP);
        item.EVP = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(item.EVP);
        item.BlockParticle = array[offset];
        offset += Marshal.SizeOf(item.BlockParticle);
        item.BlockEffect = array[offset];
        offset += Marshal.SizeOf(item.BlockEffect);
        item.ClassFlagsRaw = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(item.ClassFlagsRaw);
        item.RequiredLevel = array[offset];
        offset += Marshal.SizeOf(item.RequiredLevel);
        item.EFR = array[offset];
        offset += Marshal.SizeOf(item.EFR);
        item.ETH = array[offset];
        offset += Marshal.SizeOf(item.ETH);
        item.EIC = array[offset];
        offset += Marshal.SizeOf(item.EIC);
        item.EDK = array[offset];
        offset += Marshal.SizeOf(item.EDK);
        item.ELT = array[offset];
        offset += Marshal.SizeOf(item.ELT);
        item.DFPRange = array[offset];
        offset += Marshal.SizeOf(item.DFPRange);
        item.EVPRange = array[offset];
        offset += Marshal.SizeOf(item.EVPRange);
        return item;
    }

    public static UnitModel ToUnitModel(byte[] array, int offset)
    {
        UnitModel item = new();
        ItemBaseModel itemBase = item;
        ToItemBaseModel(array, ref offset, ref itemBase);

        item.Stat = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(item.Stat);
        item.StatAmount = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(item.StatAmount);
        item.ModifierAmount = BitConverter.ToInt16(array, offset);
        offset += Marshal.SizeOf(item.ModifierAmount);
        return item;
    }

    public static ToolModel ToToolModel(byte[] array, int offset)
    {
        ToolModel item = new();
        ItemBaseModel itemBase = item;
        ToItemBaseModel(array, ref offset, ref itemBase);

        item.Amount = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(item.Amount);
        item.Technique = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(item.Technique);
        item.Cost = BitConverter.ToInt32(array, offset);
        offset += Marshal.SizeOf(item.Cost);
        item.ItemFlag = BitConverter.ToUInt32(array, offset);
        offset += Marshal.SizeOf(item.ItemFlag);
        return item;
    }

    public static SpecialModel ToSpecialModel(byte[] array, int offset)
    {
        SpecialModel special = new()
        {
            Type = BitConverter.ToUInt16(array, offset)
        };
        offset += Marshal.SizeOf(special.Type);
        special.Amount = BitConverter.ToUInt16(array, offset);
        _ = Marshal.SizeOf(special.Amount);
        return special;
    }

    public static StatBoostModel ToStatBoostModel(byte[] array, int offset)
    {
        StatBoostModel special = new()
        {
            Stat1 = array[offset]
        };
        offset += Marshal.SizeOf(special.Stat1);
        special.Stat2 = array[offset];
        offset += Marshal.SizeOf(special.Stat2);
        special.Amount1 = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(special.Amount1);
        special.Amount2 = BitConverter.ToUInt16(array, offset);
        _ = Marshal.SizeOf(special.Amount2);
        return special;
    }
}
