using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace WinUI.Resources;

public static class AppResourceLookup
{
    public static Brush GetBrush(string resourceKey, Color fallbackColor)
    {
        if (TryGet(resourceKey, out Brush? brush))
        {
            return brush!;
        }

        return new SolidColorBrush(fallbackColor);
    }

    public static Color GetColor(string resourceKey, Color fallbackColor)
    {
        if (TryGet(resourceKey, out Color color))
        {
            return color;
        }

        return fallbackColor;
    }

    private static bool TryGet<T>(string resourceKey, out T? value)
    {
        value = default;

        if (Microsoft.UI.Xaml.Application.Current?.Resources.TryGetValue(resourceKey, out var resource) == true &&
            resource is T typedResource)
        {
            value = typedResource;
            return true;
        }

        return false;
    }
}
