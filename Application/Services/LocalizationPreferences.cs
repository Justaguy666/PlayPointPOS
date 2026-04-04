namespace Application.Services;

public sealed class LocalizationPreferences
{
    public const string DefaultLanguage = "en-US";
    public const string DefaultCurrency = "VND";
    public const string DefaultTimeZone = "+7";
    public const string DefaultDateFormat = "dd/MM/yyyy";

    public string Language { get; set; } = DefaultLanguage;
    public string Currency { get; set; } = DefaultCurrency;
    public string TimeZone { get; set; } = DefaultTimeZone;
    public string DateFormat { get; set; } = DefaultDateFormat;
}
