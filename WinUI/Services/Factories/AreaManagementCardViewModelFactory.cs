using System;
using Application.Services;
using Application.Services.Areas;
using Application.Services.Games;
using Application.Services.Products;
using Domain.Enums;
using WinUI.UIModels.Management;
using WinUI.ViewModels.AreaManagement.DetailedAreaCards;
using WinUI.ViewModels.AreaManagement.SummarizedAreaCards;
using WinUI.Services.Dialogs;

namespace WinUI.Services.Factories;

public sealed class AreaManagementCardViewModelFactory
{
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizationPreferencesService _localizationPreferencesService;
    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;
    private readonly IAreaSessionService _areaSessionService;
    private readonly IGameCatalogService _gameCatalogService;
    private readonly IProductCatalogService _productCatalogService;
    private readonly SummarizedAvailableCardViewModelFactory _availableCardViewModelFactory;
    private readonly SummarizedReservedCardViewModelFactory _reservedCardViewModelFactory;
    private readonly SummarizedRentedCardViewModelFactory _rentedCardViewModelFactory;

    public AreaManagementCardViewModelFactory(
        ILocalizationService localizationService,
        ILocalizationPreferencesService localizationPreferencesService,
        IDialogService dialogService,
        INotificationService notificationService,
        IAreaSessionService areaSessionService,
        IGameCatalogService gameCatalogService,
        IProductCatalogService productCatalogService,
        SummarizedAvailableCardViewModelFactory availableCardViewModelFactory,
        SummarizedReservedCardViewModelFactory reservedCardViewModelFactory,
        SummarizedRentedCardViewModelFactory rentedCardViewModelFactory)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _localizationPreferencesService = localizationPreferencesService ?? throw new ArgumentNullException(nameof(localizationPreferencesService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _areaSessionService = areaSessionService ?? throw new ArgumentNullException(nameof(areaSessionService));
        _gameCatalogService = gameCatalogService ?? throw new ArgumentNullException(nameof(gameCatalogService));
        _productCatalogService = productCatalogService ?? throw new ArgumentNullException(nameof(productCatalogService));
        _availableCardViewModelFactory = availableCardViewModelFactory ?? throw new ArgumentNullException(nameof(availableCardViewModelFactory));
        _reservedCardViewModelFactory = reservedCardViewModelFactory ?? throw new ArgumentNullException(nameof(reservedCardViewModelFactory));
        _rentedCardViewModelFactory = rentedCardViewModelFactory ?? throw new ArgumentNullException(nameof(rentedCardViewModelFactory));
    }

    public ISummarizedAreaCardViewModel CreateSummarized(AreaModel area)
    {
        return area.Status switch
        {
            PlayAreaStatus.Available => _availableCardViewModelFactory.Create(area),
            PlayAreaStatus.Reserved => _reservedCardViewModelFactory.Create(area),
            PlayAreaStatus.Rented => _rentedCardViewModelFactory.Create(area),
            _ => _availableCardViewModelFactory.Create(area),
        };
    }

    public object? CreateDetailed(ISummarizedAreaCardViewModel? selectedCardViewModel)
    {
        return selectedCardViewModel switch
        {
            SummarizedAvailableCardViewModel availableCardViewModel => new DetailedAvailableCardViewModel(
                _localizationService,
                _dialogService,
                availableCardViewModel.Model),
            SummarizedReservedCardViewModel reservedCardViewModel => new DetailedReservedCardViewModel(
                _localizationService,
                _localizationPreferencesService,
                _dialogService,
                _areaSessionService,
                reservedCardViewModel.Model),
            SummarizedRentedCardViewModel rentedCardViewModel => new DetailedRentedCardViewModel(
                _localizationService,
                _dialogService,
                _notificationService,
                _areaSessionService,
                _gameCatalogService,
                _productCatalogService,
                rentedCardViewModel.Model),
            _ => null,
        };
    }
}
