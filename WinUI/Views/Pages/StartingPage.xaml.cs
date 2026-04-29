using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using WinUI.Services.Layout;
using WinUI.ViewModels.Pages;

namespace WinUI.Views.Pages;

public sealed partial class StartingPage : Page
{
    private const double LogoAspectRatio = 250d / 450d;

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
        ResponsiveBreakpoint breakpoint = ResponsiveBreakpoints.FromWidth(width);
        bool isCompact = breakpoint == ResponsiveBreakpoint.Compact;

        RootGrid.Padding = isCompact
            ? new Thickness(ResponsiveBreakpoints.CompactPageHorizontalPadding, 20, ResponsiveBreakpoints.CompactPageHorizontalPadding, 20)
            : new Thickness(ResponsiveBreakpoints.PageHorizontalPadding, 32, ResponsiveBreakpoints.PageHorizontalPadding, 32);

        HeroPanel.Spacing = isCompact ? 16 : 20;
        Menu.Margin = isCompact ? new Thickness(0, 12, 0, 12) : new Thickness(0, 20, 0, 20);
        FooterPanel.Margin = isCompact ? new Thickness(0, 0, 0, 8) : new Thickness(0, 0, 0, 16);

        double horizontalBudget = Math.Max(220, width - (RootGrid.Padding.Left + RootGrid.Padding.Right));
        double maxLogoWidth = isCompact ? 320 : 450;
        double logoWidth = Math.Min(maxLogoWidth, horizontalBudget);
        Logo.Width = logoWidth;
        Logo.Height = logoWidth * LogoAspectRatio;

        double textMaxWidth = Math.Max(240, horizontalBudget);
        WelcomeText.MaxWidth = textMaxWidth;
        AppVersionText.MaxWidth = textMaxWidth;
        CopyrightText.MaxWidth = textMaxWidth;
    }
}
