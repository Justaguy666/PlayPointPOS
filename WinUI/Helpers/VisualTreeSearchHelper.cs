using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace WinUI.Helpers;

public static class VisualTreeSearchHelper
{
    public static T? FindDescendant<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
        int childCount = VisualTreeHelper.GetChildrenCount(parent);

        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is T typedChild && typedChild.Name == name)
            {
                return typedChild;
            }

            var result = FindDescendant<T>(child, name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
