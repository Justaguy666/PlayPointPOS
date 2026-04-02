using Microsoft.UI.Xaml.Controls;
using System;
using Microsoft.UI.Xaml;
using WinUI.ViewModels.Pages;

namespace WinUI.Views.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPageViewModel ViewModel { get; }

        public SettingsPage(SettingsPageViewModel viewModel)
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
