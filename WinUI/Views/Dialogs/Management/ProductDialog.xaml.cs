using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.Dialogs.Management;

namespace WinUI.Views.Dialogs.Management;

public sealed partial class ProductDialog : ContentDialog
{
    private bool _isTemporarilyHiddenForConfirmation;
    private bool _isCleanedUp;
    private readonly MainWindow _mainWindow;

    public ProductDialogViewModel ViewModel { get; }

    public IconState HeaderIconState => ViewModel.Icon;

    public IconState CloseIconState { get; } = new() { Kind = IconKind.Close, Size = 16 };

    public ProductDialog(ProductDialogViewModel viewModel, ProductDialogRequest? request, MainWindow mainWindow)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        ViewModel.Configure(request);
        DataContext = ViewModel;
        InitializeComponent();

        ViewModel.CloseRequested += HandleCloseRequested;
        ViewModel.DialogHideRequested += HandleDialogHideRequested;
        ViewModel.DialogShowRequested += HandleDialogShowRequested;
        Closed += HandleClosed;
    }

    private async void HandleBrowseImageClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".webp");
        picker.FileTypeFilter.Add(".bmp");

        nint windowHandle = WindowNative.GetWindowHandle(_mainWindow);
        InitializeWithWindow.Initialize(picker, windowHandle);

        var file = await picker.PickSingleFileAsync();
        if (file is not null)
        {
            ViewModel.ImageUriText = new Uri(file.Path).AbsoluteUri;
        }
    }

    private void HandleDialogHideRequested()
    {
        _isTemporarilyHiddenForConfirmation = true;
        Hide();
    }

    private async void HandleDialogShowRequested()
    {
        _isTemporarilyHiddenForConfirmation = false;
        await ShowAsync();
    }

    private void HandleCloseRequested()
    {
        if (_isTemporarilyHiddenForConfirmation)
        {
            Cleanup();
            return;
        }

        Hide();
    }

    private void HandleClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        if (_isTemporarilyHiddenForConfirmation)
        {
            return;
        }

        Cleanup();
    }

    private void Cleanup()
    {
        if (_isCleanedUp)
        {
            return;
        }

        _isCleanedUp = true;
        _isTemporarilyHiddenForConfirmation = false;
        Closed -= HandleClosed;
        ViewModel.CloseRequested -= HandleCloseRequested;
        ViewModel.DialogHideRequested -= HandleDialogHideRequested;
        ViewModel.DialogShowRequested -= HandleDialogShowRequested;
        ViewModel.Dispose();
    }
}
