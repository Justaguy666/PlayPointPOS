using System.Text.RegularExpressions;

namespace WinUI.Helpers.Validations;

public static partial class PhoneValidation
{
    public static bool IsValid(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return false;
        }

        string normalized = phoneNumber.Trim();
        if (!PhoneRegex().IsMatch(normalized))
        {
            return false;
        }

        int digitCount = DigitRegex().Matches(normalized).Count;
        return digitCount is >= 8 and <= 15;
    }

    [GeneratedRegex(@"^\+?[0-9][0-9\s().-]{7,24}$", RegexOptions.CultureInvariant)]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"\d", RegexOptions.CultureInvariant)]
    private static partial Regex DigitRegex();
}
