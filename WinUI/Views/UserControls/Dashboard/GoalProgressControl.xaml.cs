using System;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinUI.Views.UserControls.Dashboard;

public sealed partial class GoalProgressControl : UserControl
{
    private readonly ILocalizationService? _localizationService;

    public GoalProgressControl()
    {
        InitializeComponent();
        _localizationService = App.Host?.Services.GetService<ILocalizationService>();
        RefreshLocalizedText();
        SubscribeToLocalization();
    }

    private void SubscribeToLocalization()
    {
        if (_localizationService is null)
        {
            return;
        }

        _localizationService.LanguageChanged += HandleLanguageChanged;
        Unloaded += HandleUnloaded;
    }

    private void HandleLanguageChanged()
    {
        RefreshLocalizedText();
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        Unloaded -= HandleUnloaded;
        if (_localizationService is not null)
        {
            _localizationService.LanguageChanged -= HandleLanguageChanged;
        }
    }

    private void RefreshLocalizedText()
    {
        string separator = GetLocalizedText("ValueSeparatorText", "/");
        RevenueSeparatorRun.Text = separator;
        CustomerSeparatorRun.Text = separator;
        MemberSeparatorRun.Text = separator;
    }

    private string GetLocalizedText(string key, string fallback)
    {
        if (_localizationService is null)
        {
            return fallback;
        }

        string value = _localizationService.GetString(key);
        return string.IsNullOrWhiteSpace(value) || value.StartsWith("[", StringComparison.Ordinal) ? fallback : value;
    }
}
