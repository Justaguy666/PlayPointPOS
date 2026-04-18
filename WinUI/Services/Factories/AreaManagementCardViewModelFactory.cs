using System;
using Application.Services;
using Application.Services.Areas;
using Domain.Enums;
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
    private readonly IAreaSessionService _areaSessionService;
    private readonly SummarizedAvailableCardViewModelFactory _availableCardViewModelFactory;
    private readonly SummarizedReservedCardViewModelFactory _reservedCardViewModelFactory;
    private readonly SummarizedRentedCardViewModelFactory _rentedCardViewModelFactory;

    public AreaManagementCardViewModelFactory(
        ILocalizationService localizationService,
        ILocalizationPreferencesService localizationPreferencesService,
        IDialogService dialogService,
        INotificationService notificationService,
        IAreaSessionService areaSessionService,
        SummarizedAvailableCardViewModelFactory availableCardViewModelFactory,
        SummarizedReservedCardViewModelFactory reservedCardViewModelFactory,
        SummarizedRentedCardViewModelFactory rentedCardViewModelFactory)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _localizationPreferencesService = localizationPreferencesService ?? throw new ArgumentNullException(nameof(localizationPreferencesService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
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
                reservedCardViewModel.Model),
            SummarizedRentedCardViewModel rentedCardViewModel => new DetailedRentedCardViewModel(
                _localizationService,
                _dialogService,
                _notificationService,
                _areaSessionService,
                rentedCardViewModel.Model),
            _ => null,
        };
    }
}
