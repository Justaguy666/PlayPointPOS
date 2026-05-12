using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using WinUI.Services.Layout;
using WinUI.ViewModels.Pages;

namespace WinUI.Views.Pages;

public sealed partial class StartingPage : Page
{
    private const double LogoAspectRatio = 250d / 450d;
    private const double ShortViewportHeight = 680d;
    private const double VeryShortViewportHeight = 540d;
    private const double TinyViewportHeight = 440d;

    public StartingPageViewModel ViewModel { get; }

    public StartingPage(StartingPageViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
        SizeChanged += HandleSizeChanged;
        Unloaded += HandleUnloaded;
        ApplyResponsiveLayout();
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        SizeChanged -= HandleSizeChanged;
        Unloaded -= HandleUnloaded;
        ViewModel.Dispose();
    }

    private void HandleSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyResponsiveLayout();
    }

    private void ApplyResponsiveLayout()
    {
        double width = ActualWidth;
        double height = ActualHeight;
        ResponsiveBreakpoint breakpoint = ResponsiveBreakpoints.FromWidth(width);
        bool isCompact = breakpoint == ResponsiveBreakpoint.Compact;
        bool hasMeasuredHeight = height > 0;
        bool isShort = hasMeasuredHeight && height < ShortViewportHeight;
        bool isVeryShort = hasMeasuredHeight && height < VeryShortViewportHeight;
        bool isTiny = hasMeasuredHeight && height < TinyViewportHeight;

        if (width > 0)
        {
            RootGrid.Width = width;
        }

        if (hasMeasuredHeight)
        {
            RootGrid.MinHeight = height;
        }

        double horizontalPadding = isCompact
            ? ResponsiveBreakpoints.CompactPageHorizontalPadding
            : ResponsiveBreakpoints.PageHorizontalPadding;
        double verticalPadding = isVeryShort
            ? 12
            : isShort
                ? 20
                : isCompact
                    ? 20
                    : 32;
        RootGrid.Padding = new Thickness(horizontalPadding, verticalPadding, horizontalPadding, verticalPadding);

        HeroPanel.Spacing = isVeryShort ? 10 : isShort ? 14 : isCompact ? 16 : 20;
        Menu.Margin = isVeryShort
            ? new Thickness(0, 4, 0, 4)
            : isShort
                ? new Thickness(0, 10, 0, 10)
                : isCompact
                    ? new Thickness(0, 12, 0, 12)
                    : new Thickness(0, 20, 0, 20);
        FooterPanel.Margin = isVeryShort
            ? new Thickness(0)
            : isCompact || isShort
                ? new Thickness(0, 0, 0, 8)
                : new Thickness(0, 0, 0, 16);

        double horizontalBudget = Math.Max(220, width - (RootGrid.Padding.Left + RootGrid.Padding.Right));
        double maxLogoWidth = isCompact ? 320 : 450;
        if (isShort)
        {
            maxLogoWidth = Math.Min(maxLogoWidth, 340);
        }

        if (isVeryShort)
        {
            maxLogoWidth = Math.Min(maxLogoWidth, 260);
        }

        if (isTiny)
        {
            maxLogoWidth = Math.Min(maxLogoWidth, 220);
        }

        double logoWidth = Math.Min(maxLogoWidth, horizontalBudget);
        Logo.Width = logoWidth;
        Logo.Height = logoWidth * LogoAspectRatio;

        WelcomeText.FontSize = isVeryShort ? 20 : 24;
        AppVersionText.FontSize = isVeryShort ? 12 : 14;
        CopyrightText.FontSize = isVeryShort ? 14 : 16;

        double textMaxWidth = Math.Max(240, horizontalBudget);
        WelcomeText.MaxWidth = textMaxWidth;
        AppVersionText.MaxWidth = textMaxWidth;
        CopyrightText.MaxWidth = textMaxWidth;
    }
}
