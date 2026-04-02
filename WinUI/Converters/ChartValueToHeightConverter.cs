using Microsoft.UI.Xaml.Data;
using System;

namespace WinUI.Converters;

public class ChartValueToHeightConverter : IValueConverter
{
    private const double MaxContainerHeight = 150.0;

    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not double doubleValue)
            return null;

        if (parameter is not string maxValueStr || !double.TryParse(maxValueStr, out var maxValue))
            return doubleValue; // Fallback to original value

        // Scale the value proportionally to container height
        if (maxValue <= 0)
            return 0.0;

        var scaledHeight = (doubleValue / maxValue) * MaxContainerHeight;
        return Math.Max(scaledHeight, 2.0); // Minimum 2 pixels to be visible
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
