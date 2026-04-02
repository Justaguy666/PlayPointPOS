using System;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WinUI.ViewModels;

public abstract class LocalizedViewModelBase : ObservableObject, IDisposable
{
    private bool _isDisposed;

    protected LocalizedViewModelBase(ILocalizationService localizationService)
    {
        LocalizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        LocalizationService.LanguageChanged += HandleLanguageChanged;
        LocalizationService.CurrencyChanged += HandleCurrencyChanged;
        LocalizationService.TimeZoneChanged += HandleTimeZoneChanged;
    }

    protected ILocalizationService LocalizationService { get; }

    protected abstract void RefreshLocalizedText();

    public void Dispose()
    {
        if (_isDisposed)
            return;

        LocalizationService.LanguageChanged -= HandleLanguageChanged;
        LocalizationService.CurrencyChanged -= HandleCurrencyChanged;
        LocalizationService.TimeZoneChanged -= HandleTimeZoneChanged;
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }

    private void HandleLanguageChanged()
    {
        if (_isDisposed)
            return;

        RefreshLocalizedText();
    }

    private void HandleCurrencyChanged()
    {
        if (_isDisposed)
            return;

        RefreshLocalizedText();
    }

    private void HandleTimeZoneChanged()
    {
        if (_isDisposed)
            return;

        RefreshLocalizedText();
    }
}
