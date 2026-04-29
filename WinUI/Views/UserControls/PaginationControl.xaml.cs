using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.Services.Layout;
using WinUI.ViewModels.UserControls;

namespace WinUI.Views.UserControls;

public sealed partial class PaginationControl : UserControl
{
    public PaginationControl()
    {
        InitializeComponent();
        Loaded += HandleLoaded;
        SizeChanged += HandleSizeChanged;
        DataContextChanged += HandleDataContextChanged;
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        ApplyResponsiveLayout();
    }

    private void HandleSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ApplyResponsiveLayout();
    }

    private void HandleDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        ApplyResponsiveLayout();
    }

    private void ApplyResponsiveLayout()
    {
        double width = ActualWidth <= 0 ? ResponsiveBreakpoints.WideMinWidth : ActualWidth;
        bool isCompact = width < ResponsiveBreakpoints.MediumMinWidth;
        bool isNarrow = width < 520;

        NavigationPanel.Spacing = isCompact ? 8 : 12;
        FirstPageButton.Visibility = isNarrow ? Visibility.Collapsed : Visibility.Visible;
        LastPageButton.Visibility = isNarrow ? Visibility.Collapsed : Visibility.Visible;
        JumpToPageButtonText.Visibility = isCompact ? Visibility.Collapsed : Visibility.Visible;

        if (DataContext is not PaginationControlViewModel viewModel)
        {
            return;
        }

        int maxVisibleButtons = isNarrow ? 3 : 4;
        if (viewModel.Pagination.MaxVisiblePageButtons != maxVisibleButtons)
        {
            viewModel.Pagination.MaxVisiblePageButtons = maxVisibleButtons;
        }
    }
}
