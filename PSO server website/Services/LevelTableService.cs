using System.Runtime.InteropServices;

namespace PSOServerWebsite.Services;

public class LevelTableService(HttpClient http)
{
    public const int NumberClasses = 12;
    public const int MaxLevel = 200;
    private const int OffsetStart = 0x10;
    private const int CharacterStatsSize = 0x0E;
    private const int LevelStatsDeltaSize = 0x0C;

    public async Task<LevelTableModel> GetLevelTableAsync()
    {
        byte[] fileBytes = await http.GetByteArrayAsync("data/PlyLevelTbl.bin");

        //int readOffset = fileBytes.Length - OffsetStart;
        //OffsetModel offsetModel = new() { BaseStats = BitConverter.ToUInt32(fileBytes, readOffset), LevelDeltas = BitConverter.ToUInt32(fileBytes, readOffset + sizeof(uint)) };
        OffsetModel offsetModel = new() { BaseStats = 0, LevelDeltas = 216 }; // These are pointers we need after following all pointers.

        LevelTableModel levelTable = new();

        for (int i = 0; i < NumberClasses; i++)
        {
            levelTable.BaseStats[i] = BitConverterExtensions.ToCharacterStatsModel(fileBytes, (int)offsetModel.BaseStats + i * CharacterStatsSize);
            levelTable.LevelDeltas[i] = new LevelStatsDeltaModel[MaxLevel];
            for (int j = 0; j < MaxLevel; j++)
            {
                levelTable.LevelDeltas[i][j] = BitConverterExtensions.ToLevelStatsDeltaModel(fileBytes, (int)offsetModel.LevelDeltas + i * MaxLevel * LevelStatsDeltaSize + j * LevelStatsDeltaSize);
            }
        }

        SortClasses(levelTable);

        return levelTable;
    }

    private class OffsetModel
    {
        public uint BaseStats { get; set; }
        public uint LevelDeltas { get; set; }
    }

    private static void SortClasses(LevelTableModel levelTable)
    {
        levelTable.BaseStats = [
            levelTable.BaseStats[0], levelTable.BaseStats[1], levelTable.BaseStats[2], levelTable.BaseStats[9],
            levelTable.BaseStats[3], levelTable.BaseStats[11], levelTable.BaseStats[4], levelTable.BaseStats[5],
            levelTable.BaseStats[10], levelTable.BaseStats[6], levelTable.BaseStats[7], levelTable.BaseStats[8],
        ];

        levelTable.LevelDeltas = [
            levelTable.LevelDeltas[0], levelTable.LevelDeltas[1], levelTable.LevelDeltas[2], levelTable.LevelDeltas[9],
            levelTable.LevelDeltas[3], levelTable.LevelDeltas[11], levelTable.LevelDeltas[4], levelTable.LevelDeltas[5],
            levelTable.LevelDeltas[10], levelTable.LevelDeltas[6], levelTable.LevelDeltas[7], levelTable.LevelDeltas[8],
        ];
    }
}

public class LevelTableModel
{
    public CharacterStatsModel[] BaseStats { get; set; } = new CharacterStatsModel[LevelTableService.NumberClasses];
    public LevelStatsDeltaModel[][] LevelDeltas { get; set; } = new LevelStatsDeltaModel[LevelTableService.NumberClasses][];
}

public class CharacterStatsModel
{
    public ushort ATP { get; set; }
    public ushort MST { get; set; }
    public ushort EVP { get; set; }
    public ushort HP { get; set; }
    public ushort DFP { get; set; }
    public ushort ATA { get; set; }
    public ushort LCK { get; set; }
}

public class LevelStatsDeltaModel
{
    public byte ATP { get; set; }
    public byte MST { get; set; }
    public byte EVP { get; set; }
    public byte HP { get; set; }
    public byte DFP { get; set; }
    public byte ATA { get; set; }
    public byte LCK { get; set; }
    public byte TP { get; set; }
    public uint Experience { get; set; }
}


public static partial class BitConverterExtensions
{
    public static CharacterStatsModel ToCharacterStatsModel(byte[] array, int offset)
    {
        CharacterStatsModel model = new()
        {
            ATP = BitConverter.ToUInt16(array, offset)
        };
        offset += Marshal.SizeOf(model.ATP);
        model.MST = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(model.MST);
        model.EVP = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(model.EVP);
        model.HP = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(model.HP);
        model.DFP = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(model.DFP);
        model.ATA = BitConverter.ToUInt16(array, offset);
        offset += Marshal.SizeOf(model.ATA);
        model.LCK = BitConverter.ToUInt16(array, offset);
        _ = Marshal.SizeOf(model.LCK);
        return model;
    }

    public static LevelStatsDeltaModel ToLevelStatsDeltaModel(byte[] array, int offset)
    {
        LevelStatsDeltaModel model = new()
        {
            ATP = array[offset]
        };
        offset += Marshal.SizeOf(model.ATP);
        model.MST = array[offset];
        offset += Marshal.SizeOf(model.MST);
        model.EVP = array[offset];
        offset += Marshal.SizeOf(model.EVP);
        model.HP = array[offset];
        offset += Marshal.SizeOf(model.HP);
        model.DFP = array[offset];
        offset += Marshal.SizeOf(model.DFP);
        model.ATA = array[offset];
        offset += Marshal.SizeOf(model.ATA);
        model.LCK = array[offset];
        offset += Marshal.SizeOf(model.LCK);
        model.TP = array[offset];
        offset += Marshal.SizeOf(model.TP);
        model.Experience = BitConverter.ToUInt32(array, offset);
        _ = Marshal.SizeOf(model.Experience);
        return model;
    }
}
