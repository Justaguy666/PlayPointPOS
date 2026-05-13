using System.Globalization;
using System.Text;

namespace Application.Services;

public static class FullTextSearch
{
    public static bool Matches(string? query, params string?[] fields)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        string normalizedQuery = Normalize(query);
        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            return true;
        }

        string[] terms = normalizedQuery
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (terms.Length == 0)
        {
            return true;
        }

        string[] normalizedFields = fields
            .Where(static field => !string.IsNullOrWhiteSpace(field))
            .Select(static field => Normalize(field!))
            .Where(static field => !string.IsNullOrWhiteSpace(field))
            .ToArray();

        if (normalizedFields.Length == 0)
        {
            return false;
        }

        foreach (string term in terms)
        {
            bool foundInAnyField = normalizedFields.Any(field => field.Contains(term, StringComparison.Ordinal));
            if (!foundInAnyField)
            {
                return false;
            }
        }

        return true;
    }

    private static string Normalize(string input)
    {
        string decomposed = input
            .Replace('đ', 'd')
            .Replace('Đ', 'D')
            .Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder(decomposed.Length);
        bool previousWasSpace = false;

        foreach (char c in decomposed)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category is UnicodeCategory.NonSpacingMark
                or UnicodeCategory.SpacingCombiningMark
                or UnicodeCategory.EnclosingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(c))
            {
                builder.Append(char.ToLowerInvariant(c));
                previousWasSpace = false;
                continue;
            }

            if (char.IsWhiteSpace(c) || c is '_' or '-' or '/')
            {
                if (!previousWasSpace)
                {
                    builder.Append(' ');
                    previousWasSpace = true;
                }
            }
        }

        return builder.ToString().Trim();
    }
}
