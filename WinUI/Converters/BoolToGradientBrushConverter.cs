using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using WinUI;

namespace WinUI.Converters;

public partial class BoolToGradientBrushConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        // When IsNavigationVisible is true, use HeaderGradientBrush
        // When false (starting page), use PrimaryGradientBrush
        if (value is bool isVisible)
        {
            var resourceKey = isVisible ? "HeaderGradientBrush" : "PrimaryGradientBrush";

            if (Microsoft.UI.Xaml.Application.Current?.Resources.TryGetValue(resourceKey, out var brush) == true)
            {
                return brush as Brush;
            }
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
