using System;
using System.Threading.Tasks;
using Application.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinUI.Services;

/// <summary>
/// WinUI implementation of dialog service.
/// Depends on Microsoft.UI.Xaml (platform-specific).
/// </summary>
public class WinUIDialogService : IDialogService
{
    private FrameworkElement? _rootElement;
    private readonly Func<string, object?, ContentDialog?> _dialogFactory;
    private readonly ILocalizationService _localizationService;

    public WinUIDialogService(Func<string, object?, ContentDialog?> dialogFactory, ILocalizationService localizationService)
    {
        _dialogFactory = dialogFactory ?? throw new ArgumentNullException(nameof(dialogFactory));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }

    public void Initialize(object rootElement)
    {
        if (rootElement is FrameworkElement element)
        {
            _rootElement = element;
        }
    }

    public async Task ShowDialogAsync(string dialogKey)
        => await ShowDialogAsync(dialogKey, null);

    public async Task ShowDialogAsync(string dialogKey, object? parameter)
    {
        if (_rootElement?.XamlRoot == null)
        {
            throw new InvalidOperationException("DialogService is not initialized with a valid root element.");
        }

        var dialog = _dialogFactory(dialogKey, parameter);

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
            Title = _localizationService.GetString("DialogErrorTitleText"),
            Content = message,
            CloseButtonText = _localizationService.GetString("DialogOkButtonText"),
            XamlRoot = _rootElement.XamlRoot
        };
        await dialog.ShowAsync();
    }
}
