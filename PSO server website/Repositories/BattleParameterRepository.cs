using System.Net.Http.Json;
using System.Runtime.InteropServices;

namespace PSOServerWebsite.Repositories;

public class BattleParameterRepository(HttpClient http)
{
    private const int TableSize = 0x60;
    private const int StatOffset = 0x0;
    private const int StatSize = 0x24;
    private const int AttackOffset = 0x3600;
    private const int AttackSize = 0x30;
    private const int ResistOffset = 0x7E00;
    private const int ResistSize = 0x20;
    private static readonly string[] s_difficultyNames = ["Normal", "Hard", "Very Hard", "Ultimate"];

    public async Task<BattleParameterModel> GetBattleParameterAsync()
    {
        EpisodeEnemies[] enemies = (await http.GetFromJsonAsync<EpisodeEnemies[]>("data/enemies.json"))!;
        byte[] fileBytes = await http.GetByteArrayAsync("data/BattleParamEntry.dat");

        List<EnemyParameterModel> enemyParameters = [];
        foreach (EpisodeEnemies episode in enemies)
        {
            foreach (EnemyModel enemy in episode.Enemies)
            {
                for (int i = 0; i < s_difficultyNames.Length; i++)
                {
                    int enemyOffset = StatOffset + i * TableSize * StatSize + Convert.ToInt32(enemy.StatOffset, 16) * StatSize;
                    EnemyParameterModel enemyParameter = new(episode.Episode, s_difficultyNames[i], enemy.Id,
                        i == 3 && !string.IsNullOrEmpty(enemy.UltimateName) ? enemy.UltimateName : enemy.Name);
                    enemyParameter = ReadStat(fileBytes, enemyOffset, enemyParameter);
                    enemyOffset = AttackOffset + i * TableSize * AttackSize + enemy.Index * AttackSize;
                    enemyParameter = ReadAttack(fileBytes, enemyOffset, enemyParameter);
                    enemyOffset = ResistOffset + i * TableSize * ResistSize + Convert.ToInt32(enemy.ResistOffset, 16) * ResistSize;
                    enemyParameter = ReadResist(fileBytes, enemyOffset, enemyParameter);
                    enemyParameters.Add(enemyParameter);
                }
            }

        }
        return new BattleParameterModel { Enemies = [.. enemyParameters] };
    }

    private static EnemyParameterModel ReadStat(byte[] fileBytes, int enemyOffset, EnemyParameterModel enemyParameter)
    {
        enemyParameter.ATP = BitConverter.ToUInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.ATP);
        enemyParameter.MST = BitConverter.ToUInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.MST);
        enemyParameter.EVP = BitConverter.ToUInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.EVP);
        enemyParameter.HP = BitConverter.ToUInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.HP);
        enemyParameter.DFP = BitConverter.ToUInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.DFP);
        enemyParameter.ATA = BitConverter.ToUInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.ATA);
        enemyParameter.LCK = BitConverter.ToUInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.LCK);
        enemyParameter.ESP = BitConverter.ToUInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.ESP);
        enemyParameter.Height = BitConverter.ToSingle(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.Height);
        enemyParameter.Unknown3 = BitConverter.ToSingle(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.Unknown3);
        enemyParameter.Level = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.Level);
        enemyParameter.Experience = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.Experience);
        enemyParameter.Meseta = BitConverter.ToUInt32(fileBytes, enemyOffset);
        _ = Marshal.SizeOf(enemyParameter.Meseta);
        return enemyParameter;
    }

    private static EnemyParameterModel ReadAttack(byte[] fileBytes, int enemyOffset, EnemyParameterModel enemyParameter)
    {
        enemyParameter.Unknown1 = BitConverter.ToInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.Unknown1);
        enemyParameter.AttackATP = BitConverter.ToInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.AttackATP);
        enemyParameter.ATABonus = BitConverter.ToInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.ATABonus);
        enemyParameter.Unknown4 = BitConverter.ToInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.Unknown4);
        enemyParameter.DistanceX = BitConverter.ToSingle(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.DistanceX);
        enemyParameter.AngleX = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.AngleX);
        enemyParameter.DistanceY = BitConverter.ToSingle(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.DistanceY);
        enemyParameter.AngleY = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.AngleY);
        enemyParameter.Unknown8 = BitConverter.ToUInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.Unknown8);
        enemyParameter.Unknown9 = BitConverter.ToUInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.Unknown9);
        enemyParameter.Unknown10 = BitConverter.ToUInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.Unknown10);
        enemyParameter.Unknown11 = BitConverter.ToUInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.Unknown11);
        enemyParameter.Unknown12 = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.Unknown12);
        enemyParameter.Unknown13 = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.Unknown13);
        enemyParameter.Unknown14 = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.Unknown14);
        enemyParameter.Unknown15 = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.Unknown15);
        enemyParameter.Unknown16 = BitConverter.ToUInt32(fileBytes, enemyOffset);
        _ = Marshal.SizeOf(enemyParameter.Unknown16);
        return enemyParameter;
    }

    private static EnemyParameterModel ReadResist(byte[] fileBytes, int enemyOffset, EnemyParameterModel enemyParameter)
    {
        enemyParameter.EVPBonus = BitConverter.ToInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.EVPBonus);
        enemyParameter.EFR = BitConverter.ToUInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.EFR);
        enemyParameter.EIC = BitConverter.ToUInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.EIC);
        enemyParameter.ETH = BitConverter.ToUInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.ETH);
        enemyParameter.ELT = BitConverter.ToUInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.ELT);
        enemyParameter.EDK = BitConverter.ToUInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.EDK);
        enemyParameter.UnknownResist6 = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.UnknownResist6);
        enemyParameter.UnknownResist7 = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.UnknownResist7);
        enemyParameter.UnknownResist8 = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.UnknownResist8);
        enemyParameter.UnknownResist9 = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.UnknownResist9);
        enemyParameter.DFPBonus = BitConverter.ToInt32(fileBytes, enemyOffset);
        _ = Marshal.SizeOf(enemyParameter.DFPBonus);
        return enemyParameter;
    }

    private record EpisodeEnemies(int Episode, EnemyModel[] Enemies);
    private record EnemyModel(string Id, string Name, string? UltimateName, int Index, string StatOffset, string ResistOffset);
}

public class BattleParameterModel
{
    public EnemyParameterModel[] Enemies { get; set; } = [];
}

public class EnemyParameterModel(int episode, string difficultyName, string id, string name)
{
    public int Episode { get; set; } = episode;
    public string DifficultyName { get; set; } = difficultyName;
    public string Id { get; set; } = id;
    public string Name { get; set; } = name;
    public ushort ATP { get; set; }
    public ushort MST { get; set; }
    public ushort EVP { get; set; }
    public ushort HP { get; set; }
    public ushort DFP { get; set; }
    public ushort ATA { get; set; }
    public ushort LCK { get; set; }
    public ushort ESP { get; set; }
    public float Height { get; set; }
    public float Unknown3 { get; set; }
    public uint Level { get; set; }
    public uint Experience { get; set; }
    public uint Meseta { get; set; }

    public short Unknown1 { get; set; }
    public short AttackATP { get; set; }
    public short ATABonus { get; set; }
    public short Unknown4 { get; set; }
    public float DistanceX { get; set; }
    /// <summary>
    /// Out of 0x10000 (high 16 bits are unused)
    /// </summary>
    public uint AngleX { get; set; }
    public float DistanceY { get; set; }
    public uint AngleY { get; set; }
    public ushort Unknown8 { get; set; }
    public ushort Unknown9 { get; set; }
    public ushort Unknown10 { get; set; }
    public ushort Unknown11 { get; set; }
    public uint Unknown12 { get; set; }
    public uint Unknown13 { get; set; }
    public uint Unknown14 { get; set; }
    public uint Unknown15 { get; set; }
    public uint Unknown16 { get; set; }

    public short EVPBonus { get; set; }
    public ushort EFR { get; set; }
    public ushort EIC { get; set; }
    public ushort ETH { get; set; }
    public ushort ELT { get; set; }
    public ushort EDK { get; set; }
    public uint UnknownResist6 { get; set; }
    public uint UnknownResist7 { get; set; }
    public uint UnknownResist8 { get; set; }
    public uint UnknownResist9 { get; set; }
    public int DFPBonus { get; set; }
}
