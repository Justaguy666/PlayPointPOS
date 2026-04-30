using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;

namespace WinUI.Views.UserControls;

public sealed partial class LogoControl : UserControl
{
    private Storyboard? _rotateStoryboard;

    public LogoControl()
    {
        InitializeComponent();
        CreateStoryboard();
    }

    public string BrandNameText
    {
        get => (string)GetValue(BrandNameTextProperty);
        set => SetValue(BrandNameTextProperty, value);
    }

    public static readonly DependencyProperty BrandNameTextProperty =
        DependencyProperty.Register(
            nameof(BrandNameText),
            typeof(string),
            typeof(LogoControl),
            new PropertyMetadata("PlayPoint"));

    public string BrandSubtitleText
    {
        get => (string)GetValue(BrandSubtitleTextProperty);
        set => SetValue(BrandSubtitleTextProperty, value);
    }

    public static readonly DependencyProperty BrandSubtitleTextProperty =
        DependencyProperty.Register(
            nameof(BrandSubtitleText),
            typeof(string),
            typeof(LogoControl),
            new PropertyMetadata("BOARDGAME POS"));

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

}
