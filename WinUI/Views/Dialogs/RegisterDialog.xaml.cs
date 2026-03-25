using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.ViewModels.Dialogs;

namespace WinUI.Views.Dialogs;

public sealed partial class RegisterDialog : ContentDialog
{
    public RegisterViewModel ViewModel { get; }

    public RegisterDialog()
    {
        InitializeComponent();
        ViewModel = App.Host!.Services.GetRequiredService<RegisterViewModel>();
        DataContext = ViewModel;

        ViewModel.CloseRequested += () => Hide();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        ViewModel.Password = PasswordBox.Password;
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        ViewModel.ConfirmPassword = ConfirmPasswordBox.Password;
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        PasswordBox.Password = string.Empty;
        if (ConfirmPasswordBox != null)
        {
            ConfirmPasswordBox.Password = string.Empty;
        }
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}
