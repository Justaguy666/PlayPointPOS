using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace WinUI.Converters;

/// <summary>
/// Converts bool to Visibility inversely (true -> Collapsed, false -> Visible)
/// </summary>
public partial class BoolToInvisibleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }

        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Visibility visibility)
        {
            return visibility != Visibility.Visible;
        }

        return false;
    }
}
