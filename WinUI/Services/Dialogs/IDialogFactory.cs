using Application.Services;
using Microsoft.UI.Xaml.Controls;

namespace WinUI.Services.Dialogs;

public interface IDialogFactory
{
    ContentDialog? Create(DialogKey dialogKey, object? parameter);

    ContentDialog Create<TRequest>(TRequest request)
        where TRequest : notnull;
}
