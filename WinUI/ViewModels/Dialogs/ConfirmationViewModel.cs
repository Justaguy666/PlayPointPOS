using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WinUI.ViewModels.Dialogs;

public partial class ConfirmationViewModel : ObservableObject
{
    private readonly Action<bool> _setResultAction;

    [ObservableProperty]
    private string _titleText;

    [ObservableProperty]
    private string _messageText;

    [ObservableProperty]
    private string _confirmButtonText;

    [ObservableProperty]
    private string _cancelButtonText;

    [ObservableProperty]
    private bool _showCancelButton;

    public ConfirmationViewModel(
        string title, 
        string message, 
        string confirmButtonText, 
        string cancelButtonText, 
        bool showCancelButton,
        Action<bool> setResultAction)
    {
        _titleText = title;
        _messageText = message;
        _confirmButtonText = confirmButtonText;
        _cancelButtonText = cancelButtonText;
        _showCancelButton = showCancelButton;
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
