using System;
using System.Globalization;

namespace WinUI.Converters;

public class CurrencyConverter
{
    public string Format(decimal amount, string currencyCode, CultureInfo culture, int decimalDigits = 0)
    {
        string normalizedCurrency = NormalizeCurrency(currencyCode);
        return normalizedCurrency switch
        {
            "USD" => $"${amount.ToString($"N{Math.Max(decimalDigits, 2)}", culture)}",
            "VND" => $"{amount.ToString($"N{Math.Max(decimalDigits, 0)}", culture)} đ",
            _ => $"{amount.ToString($"N{Math.Max(decimalDigits, 0)}", culture)} {normalizedCurrency}",
        };
    }

    public string FormatCompactFromMillions(double amountInMillions, string currencyCode, CultureInfo culture, int maxFractionDigits = 1)
    {
        string valueText = Math.Abs(amountInMillions % 1) < 0.0001
            ? amountInMillions.ToString("0", culture)
            : amountInMillions.ToString($"0.{new string('#', Math.Max(maxFractionDigits, 0))}", culture);

        string normalizedCurrency = NormalizeCurrency(currencyCode);
        return normalizedCurrency switch
        {
            "USD" => $"${valueText}M",
            "VND" => $"{valueText}M đ",
            _ => $"{valueText}M {normalizedCurrency}",
        };
    }

    private static string NormalizeCurrency(string currencyCode)
        => string.IsNullOrWhiteSpace(currencyCode) ? "VND" : currencyCode.Trim().ToUpperInvariant();
}
