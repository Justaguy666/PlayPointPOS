using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using WinUI.ViewModels.Dialogs;

namespace WinUI.Views.Dialogs
{
    public sealed partial class ConfirmationDialog : ContentDialog
    {
        public ConfirmationViewModel ViewModel { get; }

        private bool _result;

        public ConfirmationDialog(
            string title,
            string message,
            string confirmButtonText,
            string cancelButtonText,
            bool showCancelButton)
        {
            ViewModel = new ConfirmationViewModel(
                title, 
                message, 
                confirmButtonText, 
                cancelButtonText, 
                showCancelButton,
                result => 
                {
                    _result = result;
                    Hide();
                });

            InitializeComponent();
        }

        public new async Task<bool> ShowAsync()
        {
            await base.ShowAsync();
            return _result;
        }
    }
}
