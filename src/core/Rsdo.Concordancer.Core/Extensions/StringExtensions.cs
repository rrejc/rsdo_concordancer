namespace Rsdo.Concordancer.Core.Extensions;

public static class StringExtensions
{
    public static bool IsWildcardSearch(this string value)
    {
        return !string.IsNullOrEmpty(value) && (value.Contains('*') || value.Contains('?'));
    }
}