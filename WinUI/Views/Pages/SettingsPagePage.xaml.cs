using Microsoft.UI.Xaml.Controls;
using System;
using Microsoft.UI.Xaml;
using WinUI.ViewModels.Pages;

namespace WinUI.Views.Pages
{
    public sealed partial class SettingsPagePage : Page
    {
        public SettingsPageViewModel ViewModel { get; }

        public SettingsPagePage(SettingsPageViewModel viewModel)
        {
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = ViewModel;
            InitializeComponent();
            Unloaded += HandleUnloaded;
        }

        private void HandleUnloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= HandleUnloaded;
            ViewModel.Dispose();
        }
    }
}
