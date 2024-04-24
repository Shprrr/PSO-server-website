using System.Text.RegularExpressions;

namespace PSOServerWebsite;

public static partial class JsonExtensions
{
    private static readonly Regex s_regex = RegexHexadecimal();
    public static string FixJson(this string json)
    {
        return s_regex.Replace(json, new MatchEvaluator((Match match) => $"\"{match}\""));
    }

    [GeneratedRegex("0x[A-Fa-f0-9]+")]
    private static partial Regex RegexHexadecimal();
}
