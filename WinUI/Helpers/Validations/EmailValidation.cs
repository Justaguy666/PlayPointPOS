using System.Text.RegularExpressions;

namespace WinUI.Helpers.Validations;

public static partial class EmailValidation
{
    public static bool IsValid(string? email)
        => !string.IsNullOrWhiteSpace(email)
        && EmailRegex().IsMatch(email.Trim());

    [GeneratedRegex(@"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();
}
