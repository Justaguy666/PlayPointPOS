using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace WinUI.Helpers;

public static class ToolTipHelper
{
    public static void ApplyMissingToolTips(DependencyObject root)
    {
        if (root is FrameworkElement element && ToolTipService.GetToolTip(element) is null)
        {
            string? tooltipText = ResolveToolTipText(element);
            if (!string.IsNullOrWhiteSpace(tooltipText))
            {
                ToolTipService.SetToolTip(element, tooltipText);
            }
        }

        int childCount = VisualTreeHelper.GetChildrenCount(root);
        for (int index = 0; index < childCount; index++)
        {
            ApplyMissingToolTips(VisualTreeHelper.GetChild(root, index));
        }
    }

    private static string? ResolveToolTipText(FrameworkElement element)
    {
        string automationName = AutomationProperties.GetName(element);
        if (!string.IsNullOrWhiteSpace(automationName))
        {
            return automationName;
        }

        return element switch
        {
            TextBlock textBlock => textBlock.Text,
            Button button => ExtractText(button.Content),
            TextBox textBox => textBox.PlaceholderText,
            PasswordBox passwordBox => passwordBox.PlaceholderText,
            ComboBox comboBox => comboBox.PlaceholderText,
            _ => null,
        };
    }

    private static string? ExtractText(object? content)
    {
        return content switch
        {
            null => null,
            string text => text,
            TextBlock textBlock => textBlock.Text,
            ContentControl contentControl => ExtractText(contentControl.Content),
            Panel panel => ExtractTextFromPanel(panel),
            _ => null,
        };
    }

    private static string? ExtractTextFromPanel(Panel panel)
    {
        foreach (UIElement child in panel.Children)
        {
            string? text = child switch
            {
                TextBlock textBlock => textBlock.Text,
                ContentControl contentControl => ExtractText(contentControl.Content),
                Panel childPanel => ExtractTextFromPanel(childPanel),
                _ => null,
            };

            if (!string.IsNullOrWhiteSpace(text))
            {
                return text;
            }
        }

        return null;
    }
}
