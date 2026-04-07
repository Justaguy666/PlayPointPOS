using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Application.Navigation;
using Application.Navigation.Requests;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels;

namespace WinUI.ViewModels.UserControls.Dashboard;

public sealed partial class PopularCardControlViewModel : LocalizedViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly string _resourcePrefix;
    private readonly string _activityFormatResourceKey;
    private readonly IReadOnlyList<PopularCardItemData> _sourceItems;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    public IconState TitleIconState { get; }

    public IconState ShowMoreIconState { get; } = new()
    {
        Kind = IconKind.More,
        Size = 24,
        AlwaysFilled = true,
    };

    public ObservableCollection<PopularCardItemViewModel> Items { get; } = [];

    public IRelayCommand ShowMoreCommand { get; }

    public PopularCardControlViewModel(
        ILocalizationService localizationService,
        INavigationService navigationService,
        string resourcePrefix,
        IconKind titleIconKind,
        string activityFormatResourceKey,
        IReadOnlyList<PopularCardItemData> sourceItems)
        : base(localizationService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _resourcePrefix = string.IsNullOrWhiteSpace(resourcePrefix)
            ? throw new ArgumentException("Resource prefix is required.", nameof(resourcePrefix))
            : resourcePrefix;
        _activityFormatResourceKey = string.IsNullOrWhiteSpace(activityFormatResourceKey)
            ? throw new ArgumentException("Activity format resource key is required.", nameof(activityFormatResourceKey))
            : activityFormatResourceKey;
        _sourceItems = sourceItems ?? throw new ArgumentNullException(nameof(sourceItems));

        TitleIconState = new IconState
        {
            Kind = titleIconKind,
            Size = 24,
            AlwaysFilled = true,
        };

        ShowMoreCommand = new RelayCommand(ShowMore);
        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        Title = LocalizationService.GetString($"{_resourcePrefix}Title");

        string activityFormat = LocalizationService.GetString(_activityFormatResourceKey);

        Items.Clear();
        foreach (var item in _sourceItems)
        {
            string activityLabel = string.Format(LocalizationService.Culture, activityFormat, item.ActivityCount);
            string amountLabel = LocalizationService.FormatCurrency(item.Amount);

            Items.Add(new PopularCardItemViewModel(
                item.Rank,
                item.Name,
                activityLabel,
                amountLabel));
        }
    }

    private void ShowMore()
    {
        INavigationRequest? navigationRequest = _resourcePrefix switch
        {
            "PopularGamesCard" => new NavigateToGameManagement(),
            "PopularFoodsCard" => new NavigateToProductManagement(),
            "PopularDrinksCard" => new NavigateToProductManagement(),
            _ => null
        };

        if (navigationRequest != null)
        {
            _navigationService.Navigate(navigationRequest);
        }
    }
}
