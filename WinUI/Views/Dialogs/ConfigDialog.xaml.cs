using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.ViewModels.Dialogs;

namespace WinUI.Views.Dialogs;

public sealed partial class ConfigDialog : ContentDialog
{
    public ConfigViewModel ViewModel { get; }

    public ConfigDialog()
    {
        InitializeComponent();
        ViewModel = App.Host!.Services.GetRequiredService<ConfigViewModel>();
        DataContext = ViewModel;

        ViewModel.CloseRequested += () => Hide();
    }

    private void ApiKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        ViewModel.ApiKey = ApiKeyBox.Password;
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}