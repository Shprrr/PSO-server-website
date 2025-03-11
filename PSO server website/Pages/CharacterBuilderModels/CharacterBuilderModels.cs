using PSOServerWebsite.Repositories;
using PSOServerWebsite.Services;
using System.Text.RegularExpressions;

namespace PSOServerWebsite.Pages.CharacterBuilderModels;

public enum ClassRace
{
    Humar,
    Hunewearl,
    Hucast,
    Hucaseal,
    Ramar,
    Ramarl,
    Racast,
    Racaseal,
    Fomar,
    Fomarl,
    Fonewm,
    Fonewearl
}

public class CharacterModel
{
    public string Name { get; set; } = "";
    public ClassRace? ClassRaceSelection { get; set; }
    public string LoadoutSelection { get; set; } = "Default";
    public Dictionary<string, LoadoutModel> Loadouts { get; set; } = new() { ["Default"] = new() };
    [System.Text.Json.Serialization.JsonIgnore]
    public LoadoutModel CurrentLoadout
    {
        get
        {
            if (!Loadouts.ContainsKey(LoadoutSelection))
                LoadoutSelection = Loadouts.Keys.First();
            return Loadouts[LoadoutSelection];
        }
    }

    public int Level { get; set; } = 1;

    public int HPMaterial { get; set; }
    public int TPMaterial { get; set; }
    public int PowerMaterial { get; set; }
    public int DefMaterial { get; set; }
    public int MindMaterial { get; set; }
    public int EvadeMaterial { get; set; }
    public int LuckMaterial { get; set; }

    public int TargetPowerMaterial { get; set; }
    public int TargetDefMaterial { get; set; }
    public int TargetMindMaterial { get; set; }
    public int TargetEvadeMaterial { get; set; }
    public int TargetLuckMaterial { get; set; }

    public int ShiftaLevel { get => CurrentLoadout.ShiftaLevel; set { CurrentLoadout.ShiftaLevel = value; } }
    public int DebandLevel { get => CurrentLoadout.DebandLevel; set { CurrentLoadout.DebandLevel = value; } }

    public string WeaponName { get => CurrentLoadout.WeaponName; set { CurrentLoadout.WeaponName = value; } }
    public string ArmorName { get => CurrentLoadout.ArmorName; set { CurrentLoadout.ArmorName = value; } }
    public string Unit1Name { get => CurrentLoadout.Unit1Name; set { CurrentLoadout.Unit1Name = value; } }
    public string Unit2Name { get => CurrentLoadout.Unit2Name; set { CurrentLoadout.Unit2Name = value; } }
    public string Unit3Name { get => CurrentLoadout.Unit3Name; set { CurrentLoadout.Unit3Name = value; } }
    public string Unit4Name { get => CurrentLoadout.Unit4Name; set { CurrentLoadout.Unit4Name = value; } }
    public string ShieldName { get => CurrentLoadout.ShieldName; set { CurrentLoadout.ShieldName = value; } }
    public string MagName { get => CurrentLoadout.MagName; set { CurrentLoadout.MagName = value; } }

    public string? Notes { get => CurrentLoadout.Notes; set { CurrentLoadout.Notes = value; } }

    public List<string> Wishlist { get; set; } = [];
}

public class LoadoutModel
{
    public int ShiftaLevel { get; set; }
    public int DebandLevel { get; set; }

    public string WeaponName { get; set; } = "";
    public string ArmorName { get; set; } = "";
    public string Unit1Name { get; set; } = "";
    public string Unit2Name { get; set; } = "";
    public string Unit3Name { get; set; } = "";
    public string Unit4Name { get; set; } = "";
    public string ShieldName { get; set; } = "";
    public string MagName { get; set; } = "Mag 5/0/0/0";

    public string? Notes { get; set; }

    public LoadoutModel() { }

    public LoadoutModel(LoadoutModel original)
    {
        ShiftaLevel = original.ShiftaLevel;
        DebandLevel = original.DebandLevel;
        WeaponName = original.WeaponName;
        ArmorName = original.ArmorName;
        Unit1Name = original.Unit1Name;
        Unit2Name = original.Unit2Name;
        Unit3Name = original.Unit3Name;
        Unit4Name = original.Unit4Name;
        ShieldName = original.ShieldName;
        MagName = original.MagName;
    }
}

public partial class WeaponModel
{
    private int _grind;
    private int _hit;

    private ItemModel? _model;

    public ItemModel? Model
    {
        get { return _model; }
        set
        {
            _model = value;
            if (_model == null) return;
            int maxGrind = _model.AsWeapon.MaxGrind;
            if (Grind > maxGrind) Grind = maxGrind;
        }
    }

    public string? Identifier => Model?.ItemIdentifier;
    public string Name => Model?.ItemName ?? "";

    public int BaseAtp => Model == null ? 0 : Model.AsWeapon.ATPMax - Model.AsWeapon.ATPMin;
    public int Atp => Model == null ? 0 : Model.AsWeapon.ATPMax + Grind * 2;
    public int Ata => Model == null ? 0 : Model.AsWeapon.ATA + Hit;
    public int Mst => Model == null ? 0 : Model.AsWeapon.MST;

    public int Grind
    {
        get { return _grind; }
        set
        {
            if (Model == null) return;
            if (value < 0) value = 0;
            int maxGrind = Model.AsWeapon.MaxGrind;
            if (value > maxGrind) value = maxGrind;
            _grind = value;
        }
    }

    public int Hit
    {
        get { return _hit; }
        set
        {
            if (value < 0) value = 0;
            if (value > 100) value = 100;
            _hit = value;
        }
    }

    public IEnumerable<Stat> GetStatBoosts() => Model?.AsWeapon.StatBoosts ?? [];

    public IEnumerable<Tech> GetTechBoosts() => Model?.AsWeapon.TechBoosts ?? [];

    public override string ToString()
    {
        if (string.IsNullOrEmpty(Name)) return "";
        string s = $"{Name}";
        if (Grind != 0) s += $" +{Grind}";
        if (Hit != 0) s += $" 0/0/0/0/{Hit}";
        return s;
    }

    public static WeaponModel Parse(ItemModel[] weapons, string weapon)
    {
        WeaponModel weaponModel = new();
        Match m = WeaponStringRegex().Match(weapon);
        if (!m.Success) return weaponModel;
        weaponModel.Model = weapons.FirstOrDefault(i => i.ItemName == m.Groups[1].Value);
        if (m.Groups[2].Success) weaponModel.Grind = int.Parse(m.Groups[2].Value);
        if (m.Groups[3].Success) weaponModel.Hit = int.Parse(m.Groups[3].Value);
        return weaponModel;
    }

    [GeneratedRegex(@"^(.+?)(?: \+(\d+?))?(?: 0/0/0/0/(\d+?))?$", RegexOptions.Singleline)]
    private static partial Regex WeaponStringRegex();
}

public class BaseArmorModel()
{
    private int _dfpBonus;
    private int _evpBonus;

    private ItemModel? _model;

    public ItemModel? Model
    {
        get { return _model; }
        set
        {
            _model = value;
            if (_model == null) return;
            int dfpBonusMax = _model.AsArmor.DFPRange;
            if (DfpBonus > dfpBonusMax) DfpBonus = dfpBonusMax;
            int evpBonusMax = _model.AsArmor.EVPRange;
            if (EvpBonus > evpBonusMax) EvpBonus = evpBonusMax;
        }
    }

    public string? Identifier => Model?.ItemIdentifier;
    public string Name => Model?.ItemName ?? "";

    public int Dfp => Model == null ? 0 : Model.AsArmor.DFP + DfpBonus;
    public int DfpBonus
    {
        get { return _dfpBonus; }
        set
        {
            if (Model == null) return;
            if (value < 0) value = 0;
            int dfpBonusMax = Model.AsArmor.DFPRange;
            if (value > dfpBonusMax) value = dfpBonusMax;
            _dfpBonus = value;
        }
    }

    public int Evp => Model == null ? 0 : Model.AsArmor.EVP + EvpBonus;
    public int EvpBonus
    {
        get { return _evpBonus; }
        set
        {
            if (Model == null) return;
            if (value < 0) value = 0;
            int evpBonusMax = Model.AsArmor.EVPRange;
            if (value > evpBonusMax) value = evpBonusMax;
            _evpBonus = value;
        }
    }

    public int Efr => Model == null ? 0 : Model.AsArmor.EFR;
    public int Eic => Model == null ? 0 : Model.AsArmor.EIC;
    public int Eth => Model == null ? 0 : Model.AsArmor.ETH;
    public int Edk => Model == null ? 0 : Model.AsArmor.EDK;
    public int Elt => Model == null ? 0 : Model.AsArmor.ELT;

    public IEnumerable<Stat> GetStatBoosts() => Model?.AsArmor.StatBoosts ?? [];

    public IEnumerable<Tech> GetTechBoosts() => Model?.AsArmor.TechBoosts ?? [];

    public override string ToString()
    {
        if (string.IsNullOrEmpty(Name)) return "";
        string s = $"{Name}";
        if (DfpBonus != 0) s += $" +{DfpBonus}DFP";
        if (EvpBonus != 0) s += $" +{EvpBonus}EVP";
        return s;
    }
}

public partial class ArmorModel() : BaseArmorModel()
{
    private int _numberSlots;

    public int NumberSlots
    {
        get { return _numberSlots; }
        set
        {
            if (Identifier == null || value < 0) value = 0;
            if (value > 4) value = 4;
            _numberSlots = value;
        }
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(Name)) return "";
        string s = $"{Name}";
        if (NumberSlots != 0) s += $" ({NumberSlots} slot{(NumberSlots > 1 ? "s" : "")})";
        if (DfpBonus != 0) s += $" +{DfpBonus}DFP";
        if (EvpBonus != 0) s += $" +{EvpBonus}EVP";
        return s;
    }

    public static ArmorModel Parse(ItemModel[] armors, string armor)
    {
        ArmorModel armorModel = new();
        Match m = ArmorStringRegex().Match(armor);
        if (!m.Success) return armorModel;
        armorModel.Model = armors.FirstOrDefault(i => i.ItemName == m.Groups[1].Value);
        if (m.Groups[2].Success) armorModel.NumberSlots = int.Parse(m.Groups[2].Value);
        if (m.Groups[3].Success) armorModel.DfpBonus = int.Parse(m.Groups[3].Value);
        if (m.Groups[4].Success) armorModel.EvpBonus = int.Parse(m.Groups[4].Value);
        return armorModel;
    }

    [GeneratedRegex(@"^(.+?)(?: \((\d{1}) slots??\))?(?: \+(\d+?)DFP)?(?: \+(\d+?)EVP)?$", RegexOptions.Singleline)]
    private static partial Regex ArmorStringRegex();
}

public partial class ShieldModel() : BaseArmorModel()
{
    public static ShieldModel Parse(ItemModel[] shields, string shield)
    {
        ShieldModel shieldModel = new();
        Match m = ShieldStringRegex().Match(shield);
        if (!m.Success) return shieldModel;
        shieldModel.Model = shields.FirstOrDefault(i => i.ItemName == m.Groups[1].Value);
        if (m.Groups[2].Success) shieldModel.DfpBonus = int.Parse(m.Groups[2].Value);
        if (m.Groups[3].Success) shieldModel.EvpBonus = int.Parse(m.Groups[3].Value);
        return shieldModel;
    }

    [GeneratedRegex(@"^(.+?)(?: \+(\d+?)DFP)?(?: \+(\d+?)EVP)?$", RegexOptions.Singleline)]
    private static partial Regex ShieldStringRegex();
}

public partial class UnitModel(ItemPMTModel itemPMT)
{
    private int _modifier;
    private ItemModel? _model;

    public ItemModel? Model
    {
        get { return _model; }
        set
        {
            _model = value;
            if (Identifier == null) return;
        }
    }

    public string? Identifier => Model?.ItemIdentifier;
    public string Name => Model?.ItemName ?? "";
    public bool CanHaveModifier => Model?.AsUnit.CanHaveModifier ?? false;

    public int Modifier
    {
        get { return CanHaveModifier ? _modifier : 0; }
        set
        {
            if (!CanHaveModifier) return;
            if (value < -2) value = -2;
            if (value > 2) value = 2;
            _modifier = value;
        }
    }

    public IEnumerable<Stat> GetStatBoosts()
    {
        if (string.IsNullOrEmpty(Identifier))
            return [];

        return itemPMT.Units[Identifier].GetStatBoosts(Modifier);
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(Name)) return "";
        string s = $"{Name}";
        if (Modifier > 0) s += new string('+', Modifier);
        if (Modifier < 0) s += new string('-', Modifier * -1);
        return s;
    }

    public static UnitModel Parse(ItemPMTModel itemPMT, ItemModel[] units, string unit)
    {
        UnitModel unitModel = new(itemPMT);
        Match m = UnitStringRegex().Match(unit);
        if (!m.Success) return unitModel;
        unitModel.Model = units.FirstOrDefault(i => i.ItemName == m.Groups[1].Value);
        if (m.Groups[2].Success) unitModel.Modifier = m.Groups[2].Value.Sum(c => c == '+' ? 1 : c == '-' ? -1 : 0);
        return unitModel;
    }

    [GeneratedRegex(@"^(.+?)([\+\-]+?)?$", RegexOptions.Singleline)]
    private static partial Regex UnitStringRegex();
}

public partial class MagModel
{
    private int _def = 5;
    private int _pow;
    private int _dex;
    private int _mind;

    public ItemModel? Model { get; set; }

    public string Name => Model?.ItemName ?? "";
    public int Level => Def + Pow + Dex + Mind;

    public int Def
    {
        get { return _def; }
        set
        {
            if (value < 5) value = 5;
            if (value > 200) value = 200;
            int total = value + Pow + Dex + Mind;
            if (total > 200) value -= total - 200;
            _def = value;
        }
    }

    public int Pow
    {
        get { return _pow; }
        set
        {
            if (value < 0) value = 0;
            if (value > 195) value = 195;
            int total = value + Def + Dex + Mind;
            if (total > 200) value -= total - 200;
            _pow = value;
        }
    }

    public int Dex
    {
        get { return _dex; }
        set
        {
            if (value < 0) value = 0;
            if (value > 195) value = 195;
            int total = value + Def + Pow + Mind;
            if (total > 200) value -= total - 200;
            _dex = value;
        }
    }

    public int Mind
    {
        get { return _mind; }
        set
        {
            if (value < 0) value = 0;
            if (value > 195) value = 195;
            int total = value + Def + Pow + Dex;
            if (total > 200) value -= total - 200;
            _mind = value;
        }
    }

    public override string ToString() => string.IsNullOrEmpty(Name) ? "" : $"{Name} {Def}/{Pow}/{Dex}/{Mind}";

    public static MagModel Parse(ItemModel[] mags, string mag)
    {
        MagModel magModel = new();
        Match m = MagStringRegex().Match(mag);
        if (!m.Success) return magModel;
        magModel.Model = mags.FirstOrDefault(i => i.ItemName == m.Groups[1].Value);
        magModel.Def = int.Parse(m.Groups[2].Value);
        magModel.Pow = int.Parse(m.Groups[3].Value);
        magModel.Dex = int.Parse(m.Groups[4].Value);
        magModel.Mind = int.Parse(m.Groups[5].Value);
        return magModel;
    }

    [GeneratedRegex(@"^(.+?) (\d+?)/(\d+?)/(\d+?)/(\d+?)$", RegexOptions.Singleline)]
    private static partial Regex MagStringRegex();
}
