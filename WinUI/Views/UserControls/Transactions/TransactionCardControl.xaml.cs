using Microsoft.UI.Xaml.Controls;
using WinUI.ViewModels.UserControls.Transactions;

namespace WinUI.Views.UserControls.Transactions;

public sealed partial class TransactionCardControl : UserControl
{
    public TransactionCardControl()
    {
        InitializeComponent();
    }

    public TransactionCardControlViewModel? ViewModel
    {
        get => DataContext as TransactionCardControlViewModel;
        set => DataContext = value;
    }
}
