using System;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinUI.Views.UserControls.Dialogs;

public sealed partial class DialogFooterActions : UserControl
{
    public DialogFooterActions()
    {
        InitializeComponent();
        Loaded += HandleLoaded;
    }

    public string SecondaryText
    {
        get => (string)GetValue(SecondaryTextProperty);
        set => SetValue(SecondaryTextProperty, value);
    }

    public static readonly DependencyProperty SecondaryTextProperty =
        DependencyProperty.Register(
            nameof(SecondaryText),
            typeof(string),
            typeof(DialogFooterActions),
            new PropertyMetadata(string.Empty));

    public ICommand? SecondaryCommand
    {
        get => (ICommand?)GetValue(SecondaryCommandProperty);
        set => SetValue(SecondaryCommandProperty, value);
    }

    public static readonly DependencyProperty SecondaryCommandProperty =
        DependencyProperty.Register(
            nameof(SecondaryCommand),
            typeof(ICommand),
            typeof(DialogFooterActions),
            new PropertyMetadata(null));

    public bool ShowSecondaryButton
    {
        get => (bool)GetValue(ShowSecondaryButtonProperty);
        set => SetValue(ShowSecondaryButtonProperty, value);
    }

    public static readonly DependencyProperty ShowSecondaryButtonProperty =
        DependencyProperty.Register(
            nameof(ShowSecondaryButton),
            typeof(bool),
            typeof(DialogFooterActions),
            new PropertyMetadata(true, HandleVisualPropertyChanged));

    public string PrimaryText
    {
        get => (string)GetValue(PrimaryTextProperty);
        set => SetValue(PrimaryTextProperty, value);
    }

    public static readonly DependencyProperty PrimaryTextProperty =
        DependencyProperty.Register(
            nameof(PrimaryText),
            typeof(string),
            typeof(DialogFooterActions),
            new PropertyMetadata(string.Empty));

    public ICommand? PrimaryCommand
    {
        get => (ICommand?)GetValue(PrimaryCommandProperty);
        set => SetValue(PrimaryCommandProperty, value);
    }

    public static readonly DependencyProperty PrimaryCommandProperty =
        DependencyProperty.Register(
            nameof(PrimaryCommand),
            typeof(ICommand),
            typeof(DialogFooterActions),
            new PropertyMetadata(null));

    public bool IsPrimaryEnabled
    {
        get => (bool)GetValue(IsPrimaryEnabledProperty);
        set => SetValue(IsPrimaryEnabledProperty, value);
    }

    public static readonly DependencyProperty IsPrimaryEnabledProperty =
        DependencyProperty.Register(
            nameof(IsPrimaryEnabled),
            typeof(bool),
            typeof(DialogFooterActions),
            new PropertyMetadata(true));

    public bool UseSuccessPrimaryStyle
    {
        get => (bool)GetValue(UseSuccessPrimaryStyleProperty);
        set => SetValue(UseSuccessPrimaryStyleProperty, value);
    }

    public static readonly DependencyProperty UseSuccessPrimaryStyleProperty =
        DependencyProperty.Register(
            nameof(UseSuccessPrimaryStyle),
            typeof(bool),
            typeof(DialogFooterActions),
            new PropertyMetadata(false, HandleVisualPropertyChanged));

    private static void HandleVisualPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is DialogFooterActions actions && actions.IsLoaded)
        {
            actions.ApplyVisualState();
        }
    }

    private void HandleLoaded(object sender, RoutedEventArgs e)
    {
        ApplyVisualState();
    }

    private void ApplyVisualState()
    {
        SecondaryButton.Visibility = ShowSecondaryButton ? Visibility.Visible : Visibility.Collapsed;
        Grid.SetColumn(PrimaryButton, ShowSecondaryButton ? 1 : 0);
        Grid.SetColumnSpan(PrimaryButton, ShowSecondaryButton ? 1 : 2);
        PrimaryButton.Style = (Style)Microsoft.UI.Xaml.Application.Current.Resources[
            UseSuccessPrimaryStyle ? "DialogSuccessFooterButtonStyle" : "DialogPrimaryFooterButtonStyle"];

        PrimaryButton.Resources.MergedDictionaries.Clear();
        PrimaryButton.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri(UseSuccessPrimaryStyle
                ? "ms-appx:///Resources/Styles/StateResources/SuccessActionButtonStateResources.xaml"
                : "ms-appx:///Resources/Styles/StateResources/PrimaryActionButtonStateResources.xaml")
        });
    }
}
