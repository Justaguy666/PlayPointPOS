using System.Globalization;

namespace Application.Services;

public interface ILocalizationService
{
    event Action LanguageChanged;
    event Action CurrencyChanged;
    event Action TimeZoneChanged;

    string Language { get; }
    string Currency { get; }
    string TimeZone { get; }
    CultureInfo Culture { get; }

    string GetString(string key);
    void ChangeLanguage(string lang);
    void ChangeCurrency(string currency);
    void ChangeTimeZone(string timeZone);
    void ApplyPreferences(string language, string currency, string timeZone);
    string FormatCurrency(decimal amount, int decimalDigits = 0);
    string FormatCompactCurrencyFromMillions(double amountInMillions, int maxFractionDigits = 1);
    string FormatHourLabel(int hour);
}
