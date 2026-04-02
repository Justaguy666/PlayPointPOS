using System;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;

namespace WinUI.Views.UserControls;

public sealed partial class LogoControl : UserControl
{
    private readonly ILocalizationService? _localizationService;
    private Storyboard? _rotateStoryboard;

    public LogoControl()
    {
        InitializeComponent();
        _localizationService = App.Host?.Services.GetService<ILocalizationService>();
        CreateStoryboard();
        RefreshLocalizedText();
        SubscribeToLocalization();
    }

    private void CreateStoryboard()
    {
        var animation = new DoubleAnimation
        {
            From = 0,
            To = 360,
            Duration = new Microsoft.UI.Xaml.Duration(TimeSpan.FromSeconds(0.6)),
            RepeatBehavior = RepeatBehavior.Forever
        };

        Storyboard.SetTarget(animation, DotsRotate);
        Storyboard.SetTargetProperty(animation, "Angle");

        _rotateStoryboard = new Storyboard();
        _rotateStoryboard.Children.Add(animation);
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        _rotateStoryboard?.Begin();
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        _rotateStoryboard?.Stop();
        DotsRotate.Angle = 0;
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

    private void HandleUnloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Unloaded -= HandleUnloaded;
        if (_localizationService is not null)
        {
            _localizationService.LanguageChanged -= HandleLanguageChanged;
        }
    }

    private void RefreshLocalizedText()
    {
        BrandNameTextBlock.Text = GetLocalizedText("BrandNameText", "PlayPoint");
        BrandSubtitleTextBlock.Text = GetLocalizedText("BrandSubtitleText", "BOARDGAME POS");
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
