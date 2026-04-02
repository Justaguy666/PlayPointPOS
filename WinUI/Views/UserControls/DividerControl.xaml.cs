using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUI.Views.UserControls;

public sealed partial class DividerControl : UserControl
{
    public DividerControl()
    {
        InitializeComponent();
    }

    public Brush DividerBrush
    {
        get => (Brush)GetValue(DividerBrushProperty);
        set => SetValue(DividerBrushProperty, value);
    }

    public static readonly DependencyProperty DividerBrushProperty =
        DependencyProperty.Register(
            nameof(DividerBrush),
            typeof(Brush),
            typeof(DividerControl),
            new PropertyMetadata(null)
        );
}
