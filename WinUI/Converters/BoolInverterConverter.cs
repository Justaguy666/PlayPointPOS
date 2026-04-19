using Microsoft.UI.Xaml.Data;
using System;

namespace WinUI.Converters;

/// <summary>
/// Inverts bool value (true -> false, false -> true)
/// </summary>
public partial class BoolInverterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }

        return true;
    }
}
