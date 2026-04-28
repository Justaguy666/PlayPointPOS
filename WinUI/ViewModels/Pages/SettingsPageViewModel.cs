using System;
using System.ComponentModel;
using Application.Services;
using WinUI.ViewModels;
using WinUI.ViewModels.UserControls;
using WinUI.ViewModels.UserControls.Settings;

namespace WinUI.ViewModels.Pages;

public partial class SettingsPageViewModel : LocalizedViewModelBase
{
    private readonly MainViewModel _mainViewModel;
    private readonly IDisposable[] _ownedViewModels;
    private bool _isDisposed;

    public SettingsPageViewModel(
        ILocalizationService localizationService,
        MainViewModel mainViewModel,
        NavbarControlViewModel accountNavigationViewModel,
        ShopInformationCardControlViewModel shopInformationCardViewModel,
        GeneralSettingsCardControlViewModel generalSettingsCardViewModel)
        : base(localizationService)
    {
        _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
        AccountNavigationViewModel = accountNavigationViewModel ?? throw new ArgumentNullException(nameof(accountNavigationViewModel));
        ShopInformationCardViewModel = shopInformationCardViewModel ?? throw new ArgumentNullException(nameof(shopInformationCardViewModel));
        GeneralSettingsCardViewModel = generalSettingsCardViewModel ?? throw new ArgumentNullException(nameof(generalSettingsCardViewModel));

        _mainViewModel.PropertyChanged += HandleMainViewModelPropertyChanged;

        _ownedViewModels =
        [
            ShopInformationCardViewModel,
            GeneralSettingsCardViewModel,
        ];
    }

    public ShopInformationCardControlViewModel ShopInformationCardViewModel { get; }

    public GeneralSettingsCardControlViewModel GeneralSettingsCardViewModel { get; }

    public NavbarControlViewModel AccountNavigationViewModel { get; }

    public bool IsCompactAccountActionsVisible => _mainViewModel.IsCompactNavigationVisible;

    protected override void RefreshLocalizedText()
    {
    }

    public new void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _mainViewModel.PropertyChanged -= HandleMainViewModelPropertyChanged;

        foreach (var viewModel in _ownedViewModels)
        {
            viewModel.Dispose();
        }

        base.Dispose();
    }

    private void HandleMainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainViewModel.IsCompactNavigationVisible)
            or nameof(MainViewModel.IsCompactNavigationMode)
            or nameof(MainViewModel.IsNavigationVisible))
        {
            OnPropertyChanged(nameof(IsCompactAccountActionsVisible));
        }
    }
}
