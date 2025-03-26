using System.Net.Http.Json;
using System.Runtime.InteropServices;

namespace PSOServerWebsite.Repositories;

public class BattleParameterRepository(HttpClient http)
{
    private const int TableSize = 0x60;
    private const int StatOffset = 0x0;
    private const int StatSize = 0x24;
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

    private record EpisodeEnemies(int Episode, EnemyModel[] Enemies);
    private record EnemyModel(string Id, string Name, string? UltimateName, string StatOffset, string ResistOffset);
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
}
