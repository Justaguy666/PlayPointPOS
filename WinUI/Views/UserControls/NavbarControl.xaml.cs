using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinUI.Views.UserControls;

public sealed partial class NavbarControl : UserControl
{
    public NavbarControl()
    {
        InitializeComponent();
    }

    private void NavbarGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (DataContext is ViewModels.UserControls.NavbarControlViewModel vm && vm.NavigationItems.Count > 0)
        {
            vm.UpdateLayout(e.NewSize.Width);
        }
    }
}
