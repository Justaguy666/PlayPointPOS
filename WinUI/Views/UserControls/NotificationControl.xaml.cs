using System;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using WinUI.ViewModels.UserControls;

namespace WinUI.Views.UserControls;

public sealed partial class NotificationControl : UserControl
{
    private readonly DispatcherTimer _autoCloseTimer;
    private readonly Storyboard _showAnimation;
    private readonly Storyboard _hideAnimation;
    private NotificationControlViewModel? _viewModel;

    public NotificationControl()
    {
        InitializeComponent();
        _showAnimation = (Storyboard)Resources["NotificationShowStoryboard"];
        _hideAnimation = (Storyboard)Resources["NotificationHideStoryboard"];
        ConfigureAnimationTargets(_showAnimation);
        ConfigureAnimationTargets(_hideAnimation);
        _hideAnimation.Completed += HideAnim_Completed;
        _autoCloseTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
        _autoCloseTimer.Tick += HandleAutoCloseTimerTick;
        DataContextChanged += HandleDataContextChanged;
        Unloaded += HandleUnloaded;
    }

    private void ConfigureAnimationTargets(Storyboard storyboard)
    {
        if (storyboard.Children.Count < 2)
        {
            throw new InvalidOperationException("Notification storyboard requires opacity and translate animations.");
        }

        // Storyboards loaded from external ResourceDictionaries don't resolve
        // local x:Name targets when started manually from code.
        Storyboard.SetTarget(storyboard.Children[0], this);
        Storyboard.SetTarget(storyboard.Children[1], RootTranslate);
    }

    private void HideAnim_Completed(object? sender, object e)
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

        _hideAnimation.Stop();
        Visibility = Visibility.Visible;
        _showAnimation.Begin();
        _autoCloseTimer.Stop();
        _autoCloseTimer.Start();
    }

    private void HideNotification()
    {
        _autoCloseTimer.Stop();

        if (Visibility == Visibility.Visible)
        {
            _showAnimation.Stop();
            _hideAnimation.Begin();
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

    private void HandleUnloaded(object? sender, RoutedEventArgs e)
    {
        DataContextChanged -= HandleDataContextChanged;
        Unloaded -= HandleUnloaded;
        _autoCloseTimer.Stop();
        _autoCloseTimer.Tick -= HandleAutoCloseTimerTick;
        _hideAnimation.Completed -= HideAnim_Completed;

        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= HandleViewModelPropertyChanged;
            _viewModel = null;
        }
    }
}
