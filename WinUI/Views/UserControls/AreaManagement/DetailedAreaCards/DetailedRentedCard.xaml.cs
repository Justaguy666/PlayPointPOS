using Microsoft.UI.Xaml.Controls;

namespace WinUI.Views.UserControls.AreaManagement.DetailedAreaCards;

public sealed partial class DetailedRentedCard : UserControl
{
    public DetailedRentedCard()
    {
        this.InitializeComponent();
    }

    private void AddGameButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is UIModels.AvailableServiceItemModel item)
        {
            if (DataContext is ViewModels.AreaManagement.DetailedAreaCards.DetailedRentedCardViewModel viewModel)
            {
                viewModel.AddGameCommand.Execute(item);
            }
        }
    }

    private void AddProductButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is UIModels.AvailableServiceItemModel item)
        {
            if (DataContext is ViewModels.AreaManagement.DetailedAreaCards.DetailedRentedCardViewModel viewModel)
            {
                viewModel.AddProductCommand.Execute(item);
            }
        }
    }}
