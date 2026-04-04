namespace Application.Services;

public interface ILocalizationPreferencesService
{
    LocalizationPreferences Preferences { get; }

    Task LoadAsync();
    Task SaveAsync(LocalizationPreferences preferences);
}
