using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinUI.Views.UserControls;

public sealed partial class NavbarControl : UserControl
{
    public bool IsWideEnough
    {
        get => (bool)GetValue(IsWideEnoughProperty);
        set => SetValue(IsWideEnoughProperty, value);
    }

    public static readonly DependencyProperty IsWideEnoughProperty =
        DependencyProperty.Register(
            nameof(IsWideEnough),
            typeof(bool),
            typeof(NavbarControl),
            new PropertyMetadata(true));

    public NavbarControl()
    {
        InitializeComponent();
    }

    private void OnNavigateClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn
            && btn.DataContext is UIModels.NavigationItemModel item
            && DataContext is ViewModels.MainViewModel vm)
        {
            vm.NavigateCommand.Execute(item);
        }
    }

    private void NavbarGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (DataContext is ViewModels.MainViewModel vm && vm.NavigationItems.Count > 0)
        {
            double itemWidth = Math.Floor(e.NewSize.Width / vm.NavigationItems.Count);
            NavGridLayout.MinItemWidth = itemWidth;

            IsWideEnough = itemWidth >= 120;
        }
    }
}
