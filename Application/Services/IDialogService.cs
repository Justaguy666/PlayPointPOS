namespace Application.Services;

public interface IDialogService
{
    Task ShowDialogAsync(string dialogKey);
    Task ShowDialogAsync(string dialogKey, object? parameter);
    Task ShowErrorAsync(string message);
    void Initialize(object rootElement);
}
