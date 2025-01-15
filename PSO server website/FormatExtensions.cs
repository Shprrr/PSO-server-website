namespace PSOServerWebsite;

public static class FormatExtensions
{
    public static string ToPercentage(this double value) => value.ToString("0.###%");
}
