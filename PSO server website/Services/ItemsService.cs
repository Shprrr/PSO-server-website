using System.ComponentModel;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace PSOServerWebsite.Services;

public class ItemsService(HttpClient http)
{
    public async Task<IEnumerable<ItemModel>> GetItemsAsync()
    {
        Dictionary<string, string>? names = await http.GetFromJsonAsync<Dictionary<string, string>>("data/names-v4.json");
        return names!.Select(n => new ItemModel(n.Key, n.Value));
    }
}

[TypeConverter(typeof(ItemModelConverter))]
public partial class ItemModel(string identifier, string name)
{
    public string Identifier { get; set; } = identifier;
    public string Name { get; set; } = name;

    public override string ToString() => $"{Name} ({Identifier})";

    public static ItemModel Parse(string s)
    {
        Match m = ParseRegex().Match(s);
        if (!m.Success) throw new FormatException();
        return new(m.Groups[2].Value, m.Groups[1].Value);
    }

    [GeneratedRegex(@"^(.+?) \(([0-9A-F]+?)\)$", RegexOptions.Singleline)]
    private static partial Regex ParseRegex();
}

internal class ItemModelConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        if (sourceType == typeof(string)) return true;
        return base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string s) return string.IsNullOrEmpty(s) ? null : ItemModel.Parse(s);
        return base.ConvertFrom(context, culture, value);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string))
            return value?.ToString() ?? null;
        return base.ConvertTo(context, culture, value, destinationType);
    }
}
