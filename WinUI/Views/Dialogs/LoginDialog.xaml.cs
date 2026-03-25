using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.ViewModels.Dialogs;

namespace WinUI.Views.Dialogs;

public sealed partial class LoginDialog : ContentDialog
{
    public LoginViewModel ViewModel { get; }

    public LoginDialog()
    {
        InitializeComponent();
        ViewModel = App.Host!.Services.GetRequiredService<LoginViewModel>();
        DataContext = ViewModel;

        ViewModel.CloseRequested += () => Hide();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        ViewModel.Password = LoginPasswordBox.Password;
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}
