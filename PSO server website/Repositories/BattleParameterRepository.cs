﻿using System.Net.Http.Json;
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
    private const int MovementOffset = 0xAE00;
    private const int MovementSize = 0x30;
    private static readonly string[] s_difficultyNames = ["Normal", "Hard", "Very Hard", "Ultimate"];

    public async Task<BattleParameterModel> GetBattleParameterAsync()
    {
        EpisodeEnemies[] enemies = (await http.GetFromJsonAsync<EpisodeEnemies[]>("data/enemies.json"))!;
        List<EnemyParameterModel> enemyParameters = [];

        byte[] fileBytes = await http.GetByteArrayAsync("data/BattleParamEntry_on.dat");
        enemyParameters = ReadEnemies(enemies, enemyParameters, fileBytes, false);

        fileBytes = await http.GetByteArrayAsync("data/BattleParamEntry.dat");
        enemyParameters = ReadEnemies(enemies, enemyParameters, fileBytes, true);

        return new BattleParameterModel { Enemies = [.. enemyParameters] };
    }

    private static List<EnemyParameterModel> ReadEnemies(EpisodeEnemies[] enemies, List<EnemyParameterModel> enemyParameters, byte[] fileBytes, bool solo)
    {
        foreach (EpisodeEnemies episode in enemies)
        {
            foreach (EnemyDataModel enemy in episode.Enemies)
            {
                for (int i = 0; i < s_difficultyNames.Length; i++)
                {
                    int enemyOffset = StatOffset + i * TableSize * StatSize + Convert.ToInt32(enemy.StatOffset, 16) * StatSize;
                    EnemyParameterModel enemyParameter = new(episode.Episode, solo, i, enemy.Id,
                        i == 3 && !string.IsNullOrEmpty(enemy.UltimateName) ? enemy.UltimateName : enemy.Name);
                    enemyParameter = ReadStat(fileBytes, enemyOffset, enemyParameter);

                    List<EnemyAttackModel> enemyAttacks = [];
                    foreach (EnemyDataAttackModel attack in enemy.Attacks ?? [])
                    {
                        enemyOffset = AttackOffset + i * TableSize * AttackSize + Convert.ToInt32(attack.AttackOffset, 16) * AttackSize;
                        enemyAttacks.Add(ReadAttack(fileBytes, enemyOffset, attack.Name));
                    }
                    enemyParameter.Attacks = [.. enemyAttacks];

                    enemyOffset = ResistOffset + i * TableSize * ResistSize + Convert.ToInt32(enemy.ResistOffset, 16) * ResistSize;
                    enemyParameter = ReadResist(fileBytes, enemyOffset, enemyParameter);
                    enemyOffset = MovementOffset + i * TableSize * MovementSize + Convert.ToInt32(enemy.ResistOffset, 16) * MovementSize;
                    enemyParameter = ReadMovement(fileBytes, enemyOffset, enemyParameter);
                    enemyParameters.Add(enemyParameter);
                }
            }
        }
        return enemyParameters;
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

    private static EnemyAttackModel ReadAttack(byte[] fileBytes, int enemyOffset, string name)
    {
        EnemyAttackModel enemyAttack = new()
        {
            Name = name,
            Unknown1 = BitConverter.ToInt16(fileBytes, enemyOffset)
        };
        enemyOffset += Marshal.SizeOf(enemyAttack.Unknown1);
        enemyAttack.AttackATP = BitConverter.ToInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyAttack.AttackATP);
        enemyAttack.ATABonus = BitConverter.ToInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyAttack.ATABonus);
        enemyAttack.Unknown4 = BitConverter.ToInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyAttack.Unknown4);
        enemyAttack.DistanceX = BitConverter.ToSingle(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyAttack.DistanceX);
        enemyAttack.AngleX = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyAttack.AngleX);
        enemyAttack.DistanceY = BitConverter.ToSingle(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyAttack.DistanceY);
        enemyAttack.AngleY = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyAttack.AngleY);
        enemyAttack.Unknown8 = BitConverter.ToUInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyAttack.Unknown8);
        enemyAttack.Unknown9 = BitConverter.ToUInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyAttack.Unknown9);
        enemyAttack.Unknown10 = BitConverter.ToUInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyAttack.Unknown10);
        enemyAttack.Unknown11 = BitConverter.ToUInt16(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyAttack.Unknown11);
        enemyAttack.Unknown12 = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyAttack.Unknown12);
        enemyAttack.Unknown13 = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyAttack.Unknown13);
        enemyAttack.Unknown14 = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyAttack.Unknown14);
        enemyAttack.Unknown15 = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyAttack.Unknown15);
        enemyAttack.Unknown16 = BitConverter.ToUInt32(fileBytes, enemyOffset);
        _ = Marshal.SizeOf(enemyAttack.Unknown16);
        return enemyAttack;
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

    private static EnemyParameterModel ReadMovement(byte[] fileBytes, int enemyOffset, EnemyParameterModel enemyParameter)
    {
        enemyParameter.IdleMoveSpeed = BitConverter.ToSingle(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.IdleMoveSpeed);
        enemyParameter.IdleAnimationSpeed = BitConverter.ToSingle(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.IdleAnimationSpeed);
        enemyParameter.MoveSpeed = BitConverter.ToSingle(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.MoveSpeed);
        enemyParameter.AnimationSpeed = BitConverter.ToSingle(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.AnimationSpeed);
        enemyParameter.UnknownMovement1 = BitConverter.ToSingle(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.UnknownMovement1);
        enemyParameter.UnknownMovement2 = BitConverter.ToSingle(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.UnknownMovement2);
        enemyParameter.UnknownMovement3 = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.UnknownMovement3);
        enemyParameter.UnknownMovement4 = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.UnknownMovement4);
        enemyParameter.UnknownMovement5 = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.UnknownMovement5);
        enemyParameter.UnknownMovement6 = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.UnknownMovement6);
        enemyParameter.UnknownMovement7 = BitConverter.ToUInt32(fileBytes, enemyOffset);
        enemyOffset += Marshal.SizeOf(enemyParameter.UnknownMovement7);
        enemyParameter.UnknownMovement8 = BitConverter.ToUInt32(fileBytes, enemyOffset);
        _ = Marshal.SizeOf(enemyParameter.UnknownMovement8);
        return enemyParameter;
    }

    private record EpisodeEnemies(int Episode, EnemyDataModel[] Enemies);
    private record EnemyDataModel(string Id, string Name, string? UltimateName, string StatOffset, string ResistOffset, EnemyDataAttackModel[] Attacks);
    private record EnemyDataAttackModel(string AttackOffset, string Name);
}

public class BattleParameterModel
{
    public EnemyParameterModel[] Enemies { get; set; } = [];
}

public class EnemyParameterModel(int episode, bool solo, int difficulty, string id, string name)
{
    public int Episode { get; set; } = episode;
    public bool Solo { get; set; } = solo;
    public int Difficulty { get; set; } = difficulty;
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

    public float IdleMoveSpeed { get; set; }
    public float IdleAnimationSpeed { get; set; }
    public float MoveSpeed { get; set; }
    public float AnimationSpeed { get; set; }
    public float UnknownMovement1 { get; set; }
    public float UnknownMovement2 { get; set; }
    public uint UnknownMovement3 { get; set; }
    public uint UnknownMovement4 { get; set; }
    public uint UnknownMovement5 { get; set; }
    public uint UnknownMovement6 { get; set; }
    public uint UnknownMovement7 { get; set; }
    public uint UnknownMovement8 { get; set; }

    public EnemyAttackModel[] Attacks { get; set; } = [];
}

public class EnemyAttackModel
{
    public required string Name { get; set; }
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
}
