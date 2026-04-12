namespace Application.Services;

public interface IDialogService
{
    Task ShowDialogAsync(string dialogKey);
    Task ShowDialogAsync(string dialogKey, object? parameter);
    Task ShowErrorAsync(string message);
    Task<bool> ShowConfirmationAsync(string titleKey, string messageKey, string confirmButtonTextKey = "DialogOkButtonText", string cancelButtonTextKey = "CancelButtonText", bool showCancelButton = true);
    void Initialize(object rootElement);
}
