using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using WinUI.Resources;

namespace WinUI.Converters;

public partial class BoolToOrangeBrushConverter : IValueConverter
{
    private readonly SolidColorBrush _orangeBrush = new(AppColors.PrimaryOrange);
    private readonly SolidColorBrush _grayBrush = new(AppColors.DarkGray);

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
