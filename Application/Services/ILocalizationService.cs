namespace Application.Services;

public interface ILocalizationService
{
    event Action LanguageChanged;
    string GetString(string key);
    void ChangeLanguage(string lang);
}
