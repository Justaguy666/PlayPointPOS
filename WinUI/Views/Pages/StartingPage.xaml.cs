using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using WinUI.ViewModels.Pages;

namespace WinUI.Views.Pages;

public sealed partial class StartingPage : Page
{
    public StartingPageViewModel ViewModel { get; }

    public StartingPage(StartingPageViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
        Unloaded += HandleUnloaded;
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        Unloaded -= HandleUnloaded;
        ViewModel.Dispose();
    }
}
