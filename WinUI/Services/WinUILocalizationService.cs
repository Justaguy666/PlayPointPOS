using System;
using System.Globalization;
using Application.Services;
using Microsoft.Windows.ApplicationModel.Resources;
using WinUI.Converters;

namespace WinUI.Services;

/// <summary>
/// WinUI implementation of localization service.
/// Uses Windows ApplicationModel Resources (platform-specific).
/// </summary>
public class WinUILocalizationService : ILocalizationService
{
    private readonly CurrencyConverter _currencyConverter;
    private readonly string _defaultTimeZone;
    private ResourceLoader _loader = new();

    public event Action? LanguageChanged;
    public event Action? CurrencyChanged;
    public event Action? TimeZoneChanged;

    public string Language { get; private set; }

    public string Currency { get; private set; }

    public string TimeZone { get; private set; }

    public CultureInfo Culture => CreateCulture(Language);

    public WinUILocalizationService(
        CurrencyConverter currencyConverter,
        string defaultLanguage,
        string defaultCurrency,
        string defaultTimeZone)
    {
        _currencyConverter = currencyConverter ?? throw new ArgumentNullException(nameof(currencyConverter));
        Language = string.IsNullOrWhiteSpace(defaultLanguage) ? "en-US" : defaultLanguage;
        Currency = string.IsNullOrWhiteSpace(defaultCurrency) ? "VND" : defaultCurrency;
        TimeZone = string.IsNullOrWhiteSpace(defaultTimeZone) ? "+7" : defaultTimeZone;
        _defaultTimeZone = TimeZone;

        ApplyThreadCulture(Language);
        Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = Language;
        _loader = new ResourceLoader();
    }

    public string GetString(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        try
        {
            return _loader.GetString(key);
        }
        catch
        {
            return $"[{key}]";
        }
    }

    public void ChangeLanguage(string lang)
    {
        if (string.IsNullOrWhiteSpace(lang) || string.Equals(Language, lang, StringComparison.OrdinalIgnoreCase))
            return;

        Language = lang.Trim();
        ApplyThreadCulture(Language);
        Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = Language;
        _loader = new ResourceLoader();
        LanguageChanged?.Invoke();
    }

    public void ChangeCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return;

        string normalized = currency.Trim().ToUpperInvariant();
        if (string.Equals(Currency, normalized, StringComparison.Ordinal))
            return;

        Currency = normalized;
        CurrencyChanged?.Invoke();
    }

    public void ChangeTimeZone(string timeZone)
    {
        if (string.IsNullOrWhiteSpace(timeZone))
            return;

        string normalized = timeZone.Trim();
        if (string.Equals(TimeZone, normalized, StringComparison.OrdinalIgnoreCase))
            return;

        TimeZone = normalized;
        TimeZoneChanged?.Invoke();
    }

    public void ApplyPreferences(string language, string currency, string timeZone)
    {
        ChangeLanguage(language);
        ChangeCurrency(currency);
        ChangeTimeZone(timeZone);
    }

    public string FormatCurrency(decimal amount, int decimalDigits = 0)
        => _currencyConverter.Format(amount, Currency, Culture, decimalDigits);

    public string FormatCompactCurrencyFromMillions(double amountInMillions, int maxFractionDigits = 1)
        => _currencyConverter.FormatCompactFromMillions(amountInMillions, Currency, Culture, maxFractionDigits);

    public string FormatHourLabel(int hour)
    {
        int localizedHour = NormalizeHour(hour + ResolveHourOffset(TimeZone) - ResolveHourOffset(_defaultTimeZone));
        return string.Format(Culture, GetString("RevenueChartHourValueFormat"), localizedHour);
    }

    private static void ApplyThreadCulture(string language)
    {
        CultureInfo culture = CreateCulture(language);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    private static CultureInfo CreateCulture(string language)
    {
        try
        {
            return CultureInfo.GetCultureInfo(language);
        }
        catch
        {
            return CultureInfo.GetCultureInfo("en-US");
        }
    }

    private static int ResolveHourOffset(string timeZone)
    {
        if (string.IsNullOrWhiteSpace(timeZone))
            return 0;

        string normalized = timeZone.Trim().ToUpperInvariant().Replace("UTC", string.Empty);
        return int.TryParse(normalized, out int hours) ? hours : 0;
    }

    private static int NormalizeHour(int hour)
    {
        int normalized = hour % 24;
        return normalized < 0 ? normalized + 24 : normalized;
    }
}
