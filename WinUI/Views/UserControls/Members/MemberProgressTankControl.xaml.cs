using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;

namespace WinUI.Views.UserControls.Members;

public sealed partial class MemberProgressTankControl : UserControl
{
    public static readonly DependencyProperty FillPercentageProperty =
        DependencyProperty.Register(
            nameof(FillPercentage),
            typeof(int),
            typeof(MemberProgressTankControl),
            new PropertyMetadata(0, OnFillPercentageChanged));

    public static readonly DependencyProperty BackgroundBrushProperty =
        DependencyProperty.Register(
            nameof(BackgroundBrush),
            typeof(Brush),
            typeof(MemberProgressTankControl),
            new PropertyMetadata(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 243, 244, 246))));

    public static readonly DependencyProperty WaterBrushProperty =
        DependencyProperty.Register(
            nameof(WaterBrush),
            typeof(Brush),
            typeof(MemberProgressTankControl),
            new PropertyMetadata(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 240, 154, 68))));

    public static readonly DependencyProperty BubbleBrushProperty =
        DependencyProperty.Register(
            nameof(BubbleBrush),
            typeof(Brush),
            typeof(MemberProgressTankControl),
            new PropertyMetadata(new SolidColorBrush(Windows.UI.Color.FromArgb(70, 255, 255, 255))));

    private bool _animationsStarted;

    private static readonly double[] BubbleRelativePositions =
    [
        0.04, 0.14, 0.26, 0.40, 0.53, 0.65, 0.77, 0.89,
    ];

    public MemberProgressTankControl()
    {
        InitializeComponent();
        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
        SizeChanged += HandleSizeChanged;
    }

    public int FillPercentage
    {
        get => (int)GetValue(FillPercentageProperty);
        set => SetValue(FillPercentageProperty, value);
    }

    public Brush BackgroundBrush
    {
        get => (Brush)GetValue(BackgroundBrushProperty);
        set => SetValue(BackgroundBrushProperty, value);
    }

    public Brush WaterBrush
    {
        get => (Brush)GetValue(WaterBrushProperty);
        set => SetValue(WaterBrushProperty, value);
    }

    public Brush BubbleBrush
    {
        get => (Brush)GetValue(BubbleBrushProperty);
        set => SetValue(BubbleBrushProperty, value);
    }

    private static void OnFillPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MemberProgressTankControl control)
        {
            control.UpdateWaterFill();
        }
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        UpdateWaterFill();

        if (_animationsStarted)
        {
            return;
        }

        _animationsStarted = true;
        GetStoryboards().ForEach(storyboard => storyboard.Begin());
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        foreach (Storyboard storyboard in GetStoryboards())
        {
            storyboard.Stop();
        }

        _animationsStarted = false;
    }

    private void HandleSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateWaterFill();
    }

    private void UpdateWaterFill()
    {
        double tankWidth = TankLayoutRoot.ActualWidth;
        double tankHeight = TankLayoutRoot.ActualHeight;

        if (tankHeight <= 0 || tankWidth <= 0)
        {
            return;
        }

        double normalizedPercentage = Math.Clamp(FillPercentage, 0, 100) / 100d;
        WaterFill.Height = tankHeight * normalizedPercentage;

        var clipRect = new Rect(0, 0, tankWidth, tankHeight);
        TankLayoutRoot.Clip = new RectangleGeometry { Rect = clipRect };
        BubbleCanvas.Clip = new RectangleGeometry { Rect = new Rect(0, 0, tankWidth, tankHeight) };

        DistributeBubbles(tankWidth);
    }

    private void DistributeBubbles(double availableWidth)
    {
        Ellipse[] bubbles =
        [
            BubbleOne, BubbleTwo, BubbleThree, BubbleFour,
            BubbleFive, BubbleSix, BubbleSeven, BubbleEight,
        ];

        for (int i = 0; i < bubbles.Length && i < BubbleRelativePositions.Length; i++)
        {
            double bubbleWidth = bubbles[i].Width;
            double maxLeft = Math.Max(0, availableWidth - bubbleWidth);
            Canvas.SetLeft(bubbles[i], BubbleRelativePositions[i] * maxLeft);
        }
    }

    private List<Storyboard> GetStoryboards()
    {
        return
        [
            (Storyboard)Resources["BubbleOneStoryboard"],
            (Storyboard)Resources["BubbleTwoStoryboard"],
            (Storyboard)Resources["BubbleThreeStoryboard"],
            (Storyboard)Resources["BubbleFourStoryboard"],
            (Storyboard)Resources["BubbleFiveStoryboard"],
            (Storyboard)Resources["BubbleSixStoryboard"],
            (Storyboard)Resources["BubbleSevenStoryboard"],
            (Storyboard)Resources["BubbleEightStoryboard"],
        ];
    }
}
