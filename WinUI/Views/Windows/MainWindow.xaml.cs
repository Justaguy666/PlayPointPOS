using System;
using System.Diagnostics;
using Application.Navigation;
using Application.Navigation.Requests;
using Microsoft.UI.Composition;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Microsoft.Graphics.Canvas.Effects;
using System.Numerics;
using WinUI.Resources;
using WinUI.Services;
using WinUI.ViewModels;
using WinUI.ViewModels.UserControls;
using WinUI.Views.Pages;
using Application.Services;

namespace WinUI;

public sealed partial class MainWindow : Window
{
    private readonly ILocalizationService _localizationService;
    public MainViewModel ViewModel { get; }
    public NavbarControlViewModel NavbarViewModel { get; }
    public NotificationControlViewModel NotificationViewModel { get; }
    private SpriteVisual? _patternVisual;

    public MainWindow(
        INavigationService nav,
        MainViewModel viewModel,
        NavbarControlViewModel navbarViewModel,
        NotificationControlViewModel notificationViewModel,
        ILocalizationService localizationService)
    {
        InitializeComponent();
        ViewModel = viewModel;
        NavbarViewModel = navbarViewModel;
        NotificationViewModel = notificationViewModel;
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

        UpdateLocalizedText();
        _localizationService.LanguageChanged += HandleLanguageChanged;

        ConfigureTitleBar();

        nav.SetFrame(MainFrame);

        if (nav is Services.WinUINavigationService winuiNav)
        {
            winuiNav.SetShellViewModels(viewModel, navbarViewModel);
        }

        SetupTilePattern();

        nav.Navigate(new NavigateToStarting());

        this.Closed += (s, e) =>
        {
            _localizationService.LanguageChanged -= HandleLanguageChanged;
            Environment.Exit(0);
        };
    }

    private void HandleLanguageChanged()
    {
        UpdateLocalizedText();
    }

    private void UpdateLocalizedText()
    {
        Title = _localizationService.GetString("AppDisplayNameText");
    }

    private void ConfigureTitleBar()
    {
        ExtendsContentIntoTitleBar = true;

        if (Content is FrameworkElement root && root.FindName("TitleBarDragRegion") is UIElement dragRegion)
        {
            SetTitleBar(dragRegion);
        }

        if (AppWindow.TitleBar is { } titleBar)
        {
            titleBar.ButtonBackgroundColor = AppColors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = AppColors.Transparent;
            titleBar.ButtonHoverBackgroundColor = AppColors.TitleBarButtonHover;
            titleBar.ButtonPressedBackgroundColor = AppColors.TitleBarButtonPressed;
            titleBar.ButtonForegroundColor = AppColors.TitleBarButton;
            titleBar.ButtonInactiveForegroundColor = AppColors.TitleBarButton;
            titleBar.ButtonHoverForegroundColor = AppColors.TitleBarButton;
            titleBar.ButtonPressedForegroundColor = AppColors.TitleBarButton;
        }
    }

    private void SetupTilePattern()
    {
        try
        {
            var compositor = ElementCompositionPreview.GetElementVisual(PatternHost).Compositor;

            var surface = LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Assets/Pattern.png"));
            var surfaceBrush = compositor.CreateSurfaceBrush(surface);

            surfaceBrush.Stretch = CompositionStretch.None;
            surfaceBrush.HorizontalAlignmentRatio = 0;
            surfaceBrush.VerticalAlignmentRatio = 0;
            surfaceBrush.TransformMatrix = Matrix3x2.CreateScale(1.0f);

            var borderEffect = new BorderEffect
            {
                Source = new CompositionEffectSourceParameter("source"),
                ExtendX = Microsoft.Graphics.Canvas.CanvasEdgeBehavior.Wrap,
                ExtendY = Microsoft.Graphics.Canvas.CanvasEdgeBehavior.Wrap
            };

            var effectFactory = compositor.CreateEffectFactory(borderEffect);
            var effectBrush = effectFactory.CreateBrush();

            effectBrush.SetSourceParameter("source", surfaceBrush);

            _patternVisual = compositor.CreateSpriteVisual();
            _patternVisual.Brush = effectBrush;
            ElementCompositionPreview.SetElementChildVisual(PatternHost, _patternVisual);

            PatternHost.SizeChanged += (s, e) =>
            {
                if (_patternVisual != null)
                {
                    _patternVisual.Size = new Vector2((float)e.NewSize.Width, (float)e.NewSize.Height);
                }
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SetupTilePattern failed: {ex}");
        }
    }

    public Visibility GetVisibility(bool isVisible)
        => isVisible ? Visibility.Visible : Visibility.Collapsed;

    public void ShowNotification(string title, string message, Application.Services.NotificationType type)
    {
        NotificationViewModel.Show(title, message, type);
    }
}
