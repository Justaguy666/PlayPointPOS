using System;
using Application.Navigation;
using Application.Navigation.Requests;
using Microsoft.UI.Composition;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Microsoft.Graphics.Canvas.Effects;
using System.Numerics;
using WinUI.ViewModels;
using WinUI.Views.Pages;

namespace WinUI;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }
    private SpriteVisual? _patternVisual;

    public MainWindow(INavigationService nav, MainViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;

        ConfigureTitleBar();

        nav.SetFrame(MainFrame);
        MainFrame.Navigated += MainFrame_Navigated;

        SetupTilePattern();

        nav.Navigate(new NavigateToStarting());

        this.Closed += (s, e) =>
        {
            Environment.Exit(0);
        };
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
            var buttonColor = ColorHelper.FromArgb(0xFF, 0x2D, 0x37, 0x48);
            var hoverBackground = ColorHelper.FromArgb(0x22, 0x2D, 0x37, 0x48);
            var pressedBackground = ColorHelper.FromArgb(0x44, 0x2D, 0x37, 0x48);

            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonHoverBackgroundColor = hoverBackground;
            titleBar.ButtonPressedBackgroundColor = pressedBackground;
            titleBar.ButtonForegroundColor = buttonColor;
            titleBar.ButtonInactiveForegroundColor = buttonColor;
            titleBar.ButtonHoverForegroundColor = buttonColor;
            titleBar.ButtonPressedForegroundColor = buttonColor;
        }
    }

    private void SetupTilePattern()
    {
        var compositor = ElementCompositionPreview.GetElementVisual(PatternHost).Compositor;

        var surface = LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Assets/pattern.png"));
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

    private void MainFrame_Navigated(object sender, NavigationEventArgs e)
    {
        ViewModel.IsNavigationVisible = e.Content is not StartingPage;
    }

    public Visibility GetVisibility(bool isVisible)
        => isVisible ? Visibility.Visible : Visibility.Collapsed;

    public void ShowNotification(string title, string message, Application.Services.NotificationType type)
    {
        NotificationToast.Show(title, message, type);
    }
}
