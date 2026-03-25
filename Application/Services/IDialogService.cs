namespace Application.Services;

public interface IDialogService
{
    Task ShowDialogAsync(string dialogKey);
    Task ShowErrorAsync(string message);
    void Initialize(object rootElement);
}
