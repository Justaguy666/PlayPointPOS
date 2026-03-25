using System;
using Application.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace WinUI.Views.UserControls;

public sealed partial class NotificationControl : UserControl
{
    private readonly DispatcherTimer _autoCloseTimer;

    public NotificationControl()
    {
        InitializeComponent();
        _autoCloseTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
        _autoCloseTimer.Tick += (s, e) => Close();
    }

    public void Show(string title, string message, NotificationType type)
    {
        TitleBlock.Text = title;
        MessageBlock.Text = message;

        switch (type)
        {
            case NotificationType.Success:
                IconContainer.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 209, 250, 229)); // #D1FAE5
                IconGlyph.Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 16, 185, 129)); // #10B981
                IconGlyph.Glyph = "\uE73E"; // Checkmark
                break;
            case NotificationType.Error:
                IconContainer.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 254, 226, 226)); // #FEE2E2
                IconGlyph.Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 239, 68, 68)); // #EF4444
                IconGlyph.Glyph = "\uEA39"; // Cancel
                break;
            case NotificationType.Warning:
                IconContainer.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 254, 243, 199)); // #FEF3C7
                IconGlyph.Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 245, 158, 11)); // #F59E0B
                IconGlyph.Glyph = "\uE7BA"; // Warning
                break;
            default:
                IconContainer.Background = new SolidColorBrush(ColorHelper.FromArgb(255, 219, 234, 254)); // #DBEAFE
                IconGlyph.Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, 59, 130, 246)); // #3B82F6
                IconGlyph.Glyph = "\uE946"; // Info
                break;
        }

        Visibility = Visibility.Visible;
        ShowAnim.Begin();

        _autoCloseTimer.Stop();
        _autoCloseTimer.Start();
    }

    public void Close()
    {
        _autoCloseTimer.Stop();
        HideAnim.Begin();
    }

    private void HideAnim_Completed(object sender, object e)
    {
        Visibility = Visibility.Collapsed;
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
