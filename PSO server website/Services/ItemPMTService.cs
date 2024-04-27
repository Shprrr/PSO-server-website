using System.Runtime.InteropServices;
using System.Text.Json;

namespace PSOServerWebsite.Services;

public class ItemPMTService(HttpClient http)
{
    // Offset from end of ItemPMT.bin file to main pointer table.
    private const int OffsetStart = 16;

    // Offsets for main table, in bytes.
    private static readonly Dictionary<string, int> s_offset = new(){
        { "weapon",            0 },
        { "armor",             4 },
        { "units",             8 },
        { "item",              12 },
        { "mag",               16 },
        { "AA",                20 },
        { "photon-color",      24 },
        { "weapon-range",      28 },
        { "weapon-sales-div",  32 },
        { "sales-div",         36 },
        { "mag-feed",          40 },
        { "star-value",        44 },
        { "special-data",      48 },
        { "weapon-SFX",        52 },
        { "stat-boost",        56 },
        { "shield-sfx",        60 },
        { "tech-use",          64 },
        { "combination",       68 },
        //{ "unknown-table",     72 },
        { "tech-bosts",        76 },
        { "unwrap",            80 },
        { "unsealable",        84 },
        { "ranged-special",    88 },
    };
    private const int WeaponClassesCount = 0xED;
    private const int WeaponSize = 0x2C;

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

        return new ItemPMTModel { Weapons = weapons };
    }
}

public class ItemPMTModel
{
    public ItemCountModel ItemCount { get; set; } = new();
    public Dictionary<string, WeaponModel> Weapons { get; set; } = [];
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
    public ushort ClassFlags { get; set; }
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

public static class BitConverterExtensions
{
    private static void ToItemBaseModel(byte[] array, ref int offset, ref ItemBaseModel itemBase)
    {
        itemBase.Id = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(itemBase.Id);
        itemBase.Type = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(itemBase.Type);
        itemBase.Skin = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(itemBase.Skin);
        itemBase.TeamPoints = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(itemBase.TeamPoints);
    }

    public static WeaponModel ToWeaponModel(byte[] array, int offset)
    {
        WeaponModel item = new();
        ItemBaseModel itemBase = item;
        ToItemBaseModel(array, ref offset, ref itemBase);

        item.ClassFlags = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(item.ClassFlags);
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
}
