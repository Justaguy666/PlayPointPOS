using Application.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Infrastructure.Services.Dialog;

public class DialogService : IDialogService
{
    private FrameworkElement? _rootElement;
    private readonly Func<string, ContentDialog?> _dialogFactory;

    public DialogService(Func<string, ContentDialog?> dialogFactory)
    {
        _dialogFactory = dialogFactory;
    }

    public void Initialize(object rootElement)
    {
        if (rootElement is FrameworkElement element)
        {
            _rootElement = element;
        }
    }

    public async Task ShowDialogAsync(string dialogKey)
    {
        if (_rootElement?.XamlRoot == null)
        {
            throw new InvalidOperationException("DialogService is not initialized with a valid root element.");
        }

        var dialog = _dialogFactory(dialogKey);

        if (dialog != null)
        {
            dialog.XamlRoot = _rootElement.XamlRoot;
            await dialog.ShowAsync();
        }
    }

    public async Task ShowErrorAsync(string message)
    {
        if (_rootElement?.XamlRoot == null)
        {
            throw new InvalidOperationException("DialogService is not initialized with a valid root element.");
        }

        var dialog = new ContentDialog
        {
            Title = "Error",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = _rootElement.XamlRoot
        };
        await dialog.ShowAsync();
    }
}
