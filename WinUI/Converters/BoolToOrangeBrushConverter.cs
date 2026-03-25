using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace WinUI.Converters;

public partial class BoolToOrangeBrushConverter : IValueConverter
{
    private readonly SolidColorBrush _orangeBrush = new(Windows.UI.Color.FromArgb(255, 255, 107, 53)); // #FF6B35
    private readonly SolidColorBrush _grayBrush = new(Windows.UI.Color.FromArgb(255, 160, 174, 192)); // #A0AEC0

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isSelected && isSelected)
        {
            return _orangeBrush;
        }

        return _grayBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
