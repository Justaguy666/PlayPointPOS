using System;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.ViewModels.UserControls;

namespace WinUI.Views.UserControls;

public sealed partial class NotificationControl : UserControl
{
    private readonly DispatcherTimer _autoCloseTimer;
    private NotificationControlViewModel? _viewModel;

    public NotificationControl()
    {
        InitializeComponent();
        _autoCloseTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
        _autoCloseTimer.Tick += HandleAutoCloseTimerTick;
        DataContextChanged += HandleDataContextChanged;
        Unloaded += HandleUnloaded;
    }

    private void HideAnim_Completed(object sender, object e)
    {
        Visibility = Visibility.Collapsed;
    }

    private void HandleDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= HandleViewModelPropertyChanged;
        }

        _viewModel = args.NewValue as NotificationControlViewModel;

        if (_viewModel != null)
        {
            _viewModel.PropertyChanged += HandleViewModelPropertyChanged;
            SyncVisibility();
        }
        else
        {
            _autoCloseTimer.Stop();
            Visibility = Visibility.Collapsed;
        }
    }

    private void HandleViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_viewModel == null)
        {
            return;
        }

        if (e.PropertyName == nameof(NotificationControlViewModel.DisplayVersion))
        {
            DispatcherQueue.TryEnqueue(ShowNotification);
            return;
        }

        if (e.PropertyName == nameof(NotificationControlViewModel.IsVisible) && !_viewModel.IsVisible)
        {
            DispatcherQueue.TryEnqueue(HideNotification);
        }
    }

    private void ShowNotification()
    {
        if (_viewModel == null || !_viewModel.IsVisible)
        {
            return;
        }

        HideAnim.Stop();
        Visibility = Visibility.Visible;
        ShowAnim.Begin();
        _autoCloseTimer.Stop();
        _autoCloseTimer.Start();
    }

    private void HideNotification()
    {
        _autoCloseTimer.Stop();

        if (Visibility == Visibility.Visible)
        {
            ShowAnim.Stop();
            HideAnim.Begin();
        }
        else
        {
            Visibility = Visibility.Collapsed;
        }
    }

    private void SyncVisibility()
    {
        if (_viewModel?.IsVisible == true)
        {
            ShowNotification();
            return;
        }

        _autoCloseTimer.Stop();
        Visibility = Visibility.Collapsed;
    }

    private void HandleAutoCloseTimerTick(object? sender, object e)
    {
        _viewModel?.Close();
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        DataContextChanged -= HandleDataContextChanged;
        Unloaded -= HandleUnloaded;
        _autoCloseTimer.Stop();
        _autoCloseTimer.Tick -= HandleAutoCloseTimerTick;

        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= HandleViewModelPropertyChanged;
            _viewModel = null;
        }
    }
}
