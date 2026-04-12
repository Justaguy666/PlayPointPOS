using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using WinUI.Resources;

namespace WinUI.Converters;

public partial class BoolToAccentForegroundBrushConverter : IValueConverter
{
    private readonly SolidColorBrush _accentBrush = new(AppColors.PrimaryOrange);
    private readonly SolidColorBrush _defaultBrush = new(AppColors.Black);

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isAccent && isAccent)
        {
            return _accentBrush;
        }

        return _defaultBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
