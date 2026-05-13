using System;
using System.Diagnostics;
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

    public XamlRoot? TryGetXamlRoot() => _rootElement?.XamlRoot;

    public async Task ShowDialogAsync(string dialogKey)
        => await ShowDialogAsync(dialogKey, null);

    public async Task ShowDialogAsync(string dialogKey, object? parameter)
    {
        const string tag = "[DialogService]";
        try
        {
            if (_rootElement?.XamlRoot is null)
            {
                Debug.WriteLine($"{tag} BOUNDARY-A: XamlRoot is null — IDialogService.Initialize was not called with a live FrameworkElement, or Content is not in the tree yet.");
                return;
            }

            Debug.WriteLine($"{tag} BOUNDARY-B: dialogKey={dialogKey}, parameterType={parameter?.GetType().Name ?? "null"}");

            ContentDialog? dialog;
            try
            {
                dialog = _dialogFactory(dialogKey, parameter);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{tag} BOUNDARY-C: factory threw for '{dialogKey}': {ex}");
                await ShowSafeUserErrorAsync($"Tạo dialog thất bại ({dialogKey})\n{ex.Message}");
                return;
            }

            if (dialog is null)
            {
                Debug.WriteLine($"{tag} BOUNDARY-D: factory returned null for '{dialogKey}'.");
                await ShowSafeUserErrorAsync($"Không có dialog đăng ký cho '{dialogKey}'.");
                return;
            }

            Debug.WriteLine($"{tag} BOUNDARY-E: showing {dialog.GetType().FullName} …");
            dialog.XamlRoot = _rootElement.XamlRoot;

            try
            {
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{tag} BOUNDARY-F: ShowAsync failed for '{dialogKey}': {ex}");
                await ShowSafeUserErrorAsync($"Không mở được dialog ({dialogKey})\n{ex.Message}");
                return;
            }

            Debug.WriteLine($"{tag} BOUNDARY-G: ShowAsync finished for '{dialogKey}'.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{tag} BOUNDARY-Z: unexpected: {ex}");
            await ShowSafeUserErrorAsync(ex.Message);
        }
    }

    private async Task ShowSafeUserErrorAsync(string message)
    {
        try
        {
            if (_rootElement?.XamlRoot is not null)
            {
                await ShowErrorAsync(message);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DialogService] ShowErrorAsync failed: {ex}");
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

    public async Task<bool> ShowConfirmationAsync(string titleKey, string messageKey, string confirmButtonTextKey = "DialogOkButtonText", string cancelButtonTextKey = "CancelButtonText", bool showCancelButton = true)
    {
        if (_rootElement?.XamlRoot == null)
        {
            throw new InvalidOperationException("DialogService is not initialized with a valid root element.");
        }

        var title = _localizationService.GetString(titleKey);
        var message = _localizationService.GetString(messageKey);
        var confirmButtonText = _localizationService.GetString(confirmButtonTextKey);
        var cancelButtonText = _localizationService.GetString(cancelButtonTextKey);

        var dialog = new Views.Dialogs.ConfirmationDialog(title, message, confirmButtonText, cancelButtonText, showCancelButton);
        dialog.XamlRoot = _rootElement.XamlRoot;
        return await dialog.ShowAsync();
    }
}
