using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace PSOServerWebsite.Repositories;

public class ItemsRepository(HttpClient http)
{
    private static Task<Dictionary<string, string>>? s_namesJson = null;

    [MemberNotNull(nameof(s_namesJson))]
    public void LoadData()
    {
        s_namesJson ??= http.GetFromJsonAsync<Dictionary<string, string>>("data/names-v4.json")!;
    }

    public async Task<IEnumerable<ItemNameModel>> GetItemsAsync()
    {
        LoadData();
        return (await s_namesJson).Select(n => new ItemNameModel(n.Key, n.Value));
    }
}

[TypeConverter(typeof(ItemNameModelConverter))]
public partial class ItemNameModel(string identifier, string name)
{
    public string Identifier { get; set; } = identifier;
    public string Name { get; set; } = name;

    public override string ToString() => $"{Name} ({Identifier})";

    public static ItemNameModel Parse(string s)
    {
        Match m = ParseRegex().Match(s);
        if (!m.Success) throw new FormatException();
        return new(m.Groups[2].Value, m.Groups[1].Value);
    }

    [GeneratedRegex(@"^(.+?) \(([0-9A-F]+?)\)$", RegexOptions.Singleline)]
    private static partial Regex ParseRegex();
}

internal class ItemNameModelConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        if (sourceType == typeof(string)) return true;
        return base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string s) return string.IsNullOrEmpty(s) ? null : ItemNameModel.Parse(s);
        return base.ConvertFrom(context, culture, value);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string))
            return value?.ToString() ?? null;
        return base.ConvertTo(context, culture, value, destinationType);
    }
}
