using System.Threading.Tasks;

namespace WinUI.Services.Dialogs;

public interface IDialogService
{
    Task ShowDialogAsync(DialogKey dialogKey);

    Task ShowDialogAsync(DialogKey dialogKey, object? parameter);

    Task ShowDialogAsync<TRequest>(TRequest request)
        where TRequest : notnull;

    Task ShowErrorAsync(string message);

    Task<bool> ShowConfirmationAsync(
        string titleKey,
        string messageKey,
        string confirmButtonTextKey = "DialogOkButtonText",
        string cancelButtonTextKey = "CancelButtonText",
        bool showCancelButton = true);

    void Initialize(object rootElement);
}
