using Application.Services;
using Microsoft.Windows.ApplicationModel.Resources;

namespace Infrastructure.Services;

public class LocalizationService : ILocalizationService
{
    private ResourceLoader _loader = new();

    public event Action? LanguageChanged;

    public string GetString(string key)
    {
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
        Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = lang;
        _loader = new ResourceLoader();
        LanguageChanged?.Invoke();
    }
}
