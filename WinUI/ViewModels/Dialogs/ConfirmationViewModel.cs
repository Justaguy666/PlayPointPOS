using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WinUI.ViewModels.Dialogs;

public partial class ConfirmationViewModel : ObservableObject
{
    private readonly Action<bool> _setResultAction;

    [ObservableProperty]
    public partial string TitleText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MessageText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConfirmButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CancelButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool ShowCancelButton { get; set; }

    public ConfirmationViewModel(
        string title, 
        string message, 
        string confirmButtonText, 
        string cancelButtonText, 
        bool showCancelButton,
        Action<bool> setResultAction)
    {
        TitleText = title;
        MessageText = message;
        ConfirmButtonText = confirmButtonText;
        CancelButtonText = cancelButtonText;
        ShowCancelButton = showCancelButton;
        _setResultAction = setResultAction;
    }

    [RelayCommand]
    private void Confirm()
    {
        _setResultAction(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        _setResultAction(false);
    }
}
