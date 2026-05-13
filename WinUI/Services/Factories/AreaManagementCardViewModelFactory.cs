using System;
using Application.Services;
using Application.Services.Areas;
using Application.Services.Products;
using Application.Services.Games;
using Domain.Enums;
using WinUI.Services;
using WinUI.UIModels.Management;
using WinUI.ViewModels.AreaManagement.DetailedAreaCards;
using WinUI.ViewModels.AreaManagement.SummarizedAreaCards;

namespace WinUI.Services.Factories;

public sealed class AreaManagementCardViewModelFactory
{
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizationPreferencesService _localizationPreferencesService;
    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;
    private readonly IManagementApiService _managementApiService;
    private readonly IAreaSessionService _areaSessionService;
    private readonly SessionSalePickerService _sessionSalePicker;
    private readonly IProductCatalogService _productCatalogService;
    private readonly IGameCatalogService _gameCatalogService;
    private readonly SummarizedAvailableCardViewModelFactory _availableCardViewModelFactory;
    private readonly SummarizedReservedCardViewModelFactory _reservedCardViewModelFactory;
    private readonly SummarizedRentedCardViewModelFactory _rentedCardViewModelFactory;

    public AreaManagementCardViewModelFactory(
        ILocalizationService localizationService,
        ILocalizationPreferencesService localizationPreferencesService,
        IDialogService dialogService,
        INotificationService notificationService,
        IManagementApiService managementApiService,
        SessionSalePickerService sessionSalePicker,
        IProductCatalogService productCatalogService,
        IGameCatalogService gameCatalogService,
        IAreaSessionService areaSessionService,
        SummarizedAvailableCardViewModelFactory availableCardViewModelFactory,
        SummarizedReservedCardViewModelFactory reservedCardViewModelFactory,
        SummarizedRentedCardViewModelFactory rentedCardViewModelFactory)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _localizationPreferencesService = localizationPreferencesService ?? throw new ArgumentNullException(nameof(localizationPreferencesService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _managementApiService = managementApiService ?? throw new ArgumentNullException(nameof(managementApiService));
        _sessionSalePicker = sessionSalePicker ?? throw new ArgumentNullException(nameof(sessionSalePicker));
        _productCatalogService = productCatalogService ?? throw new ArgumentNullException(nameof(productCatalogService));
        _gameCatalogService = gameCatalogService ?? throw new ArgumentNullException(nameof(gameCatalogService));
        _areaSessionService = areaSessionService ?? throw new ArgumentNullException(nameof(areaSessionService));
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
        if (selectedCardViewModel is null)
        {
            return null;
        }

        AreaModel model = selectedCardViewModel.Model;
        return selectedCardViewModel.Status switch
        {
            PlayAreaStatus.Available => new DetailedAvailableCardViewModel(
                _localizationService,
                _dialogService,
                _notificationService,
                model),
            PlayAreaStatus.Reserved => new DetailedReservedCardViewModel(
                _localizationService,
                _localizationPreferencesService,
                _dialogService,
                _managementApiService,
                _notificationService,
                model),
            PlayAreaStatus.Rented => new DetailedRentedCardViewModel(
                _localizationService,
                _dialogService,
                _areaSessionService,
                _sessionSalePicker,
                _productCatalogService,
                _gameCatalogService,
                model),
            _ => new DetailedAvailableCardViewModel(
                _localizationService,
                _dialogService,
                _notificationService,
                model),
        };
    }
}
