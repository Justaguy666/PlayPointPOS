using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using WinUI.ViewModels;

namespace WinUI.Views.Pages;

public sealed partial class StartingPage : Page
{
    public StartingViewModel ViewModel { get; }

    public StartingPage()
    {
        InitializeComponent();
        ViewModel = App.Host!.Services.GetRequiredService<StartingViewModel>();
        DataContext = ViewModel;
    }
}
