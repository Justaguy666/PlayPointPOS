using System;
using Microsoft.UI.Xaml.Data;

namespace WinUI.Converters;

public partial class DivideByCountConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double width
            && parameter is string countStr
            && double.TryParse(countStr, out double count)
            && count > 0)
        {
            return width / count;
        }

        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
