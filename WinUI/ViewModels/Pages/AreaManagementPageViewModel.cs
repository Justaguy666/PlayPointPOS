using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinUI.Services.Factories;
using WinUI.UIModels;
using WinUI.UIModels.AreaManagement.SummarizedAreaCards;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.AreaManagement.DetailedAreaCards;
using WinUI.ViewModels.AreaManagement.SummarizedAreaCards;

namespace WinUI.ViewModels.Pages;

public partial class AreaManagementPageViewModel : LocalizedViewModelBase
{
    private readonly IDisposable[] _ownedViewModels;
    private bool _isDisposed;

    [ObservableProperty]
    public partial string ListHeaderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AddAreaButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IconState AddIconState { get; set; } = new()
    {
        Kind = IconKind.Add,
        Size = 24,
    };

    [ObservableProperty]
    public partial string FilterAreaButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IconState FilterIconState { get; set; } = new()
    {
        Kind = IconKind.Filter,
        Size = 24,
    };

    [ObservableProperty]
    public partial string AllAreasFilterText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TableFilterText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IconState TableFilterIconState { get; set; } = new()
    {
        Kind = IconKind.Table,
        Size = 24,
    };

    [ObservableProperty]
    public partial string RoomFilterText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IconState RoomFilterIconState { get; set; } = new()
    {
        Kind = IconKind.Room,
        Size = 24,
    };

    [ObservableProperty]
    public partial IconState NoAreasIconState { get; set; } = new()
    {
        Kind = IconKind.Dice,
        AlwaysFilled = true,
        Size = 48,
    };

    [ObservableProperty]
    public partial string NoAreasText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsAnySummarizedCardSelected { get; set; }

    [ObservableProperty]
    public partial object? DetailedAreaCardViewModel { get; set; }

    public AreaManagementPageViewModel(
        ILocalizationService localizationService,
        IDialogService dialogService,
        SummarizedAvailableCardViewModelFactory availableCardViewModelFactory,
        SummarizedReservedCardViewModelFactory reservedCardViewModelFactory,
        SummarizedRentedCardViewModelFactory rentedCardViewModelFactory)
        : base(localizationService)
    {
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(availableCardViewModelFactory);
        ArgumentNullException.ThrowIfNull(reservedCardViewModelFactory);
        ArgumentNullException.ThrowIfNull(rentedCardViewModelFactory);

        AddAreaCommand = new AsyncRelayCommand(ExecuteNoopAsync);
        FilterAreaCommand = new AsyncRelayCommand(ExecuteNoopAsync);
        AllAreasCommand = new AsyncRelayCommand(ExecuteNoopAsync);
        TableFilterCommand = new AsyncRelayCommand(ExecuteNoopAsync);
        RoomFilterCommand = new AsyncRelayCommand(ExecuteNoopAsync);
        EditAreaCommand = new AsyncRelayCommand(ExecuteNoopAsync);
        DeleteAreaCommand = new AsyncRelayCommand(ExecuteNoopAsync);
        SelectSummarizedAreaCardCommand = new RelayCommand<ISummarizedAreaCardViewModel?>(SelectSummarizedAreaCard);

        var mockCards = new[]
        {
            new SummarizedAvailableCardModel { AreaName = "Ban A01" },
            new SummarizedAvailableCardModel { AreaName = "Ban A02" },
            new SummarizedAvailableCardModel { AreaName = "Ban B01" },
            new SummarizedAvailableCardModel { AreaName = "Phong VIP 01" },
            new SummarizedAvailableCardModel { AreaName = "Phong VIP 02" },
            new SummarizedAvailableCardModel { AreaName = "Ban S05" },
        };

        SummarizedAvailableCardViewModels = new ObservableCollection<SummarizedAvailableCardViewModel>(
            mockCards.Select(availableCardViewModelFactory.Create));

        var mockReservedCards = new[]
        {
            new SummarizedReservedCardModel
            {
                AreaName = "Phong R01",
                CustomerName = "Tran Minh Anh",
                CheckInTime = DateTime.Today.AddHours(13).AddMinutes(30),
                Capacity = 6,
            },
            new SummarizedReservedCardModel
            {
                AreaName = "Ban C02",
                CustomerName = "Nguyen Hoang Long",
                CheckInTime = DateTime.Today.AddHours(15),
                Capacity = 4,
            },
            new SummarizedReservedCardModel
            {
                AreaName = "Phong VIP 03",
                CustomerName = "Le Thu Ha",
                CheckInTime = DateTime.Today.AddHours(18).AddMinutes(15),
                Capacity = 8,
            },
        };

        SummarizedReservedCardViewModels = new ObservableCollection<SummarizedReservedCardViewModel>(
            mockReservedCards.Select(reservedCardViewModelFactory.Create));

        var mockRentedCards = new[]
        {
            new SummarizedRentedCardModel
            {
                AreaName = "Ban D01",
                Capacity = 4,
                StartTime = DateTime.UtcNow - TimeSpan.FromMinutes(12),
                TotalAmount = 0,
            },
             new SummarizedRentedCardModel
            {
                AreaName = "Ban D02",
                Capacity = 4,
                StartTime = DateTime.UtcNow - TimeSpan.FromHours(1),
                TotalAmount = 40000,
            },
             new SummarizedRentedCardModel
            {
                AreaName = "Phong VIP 04",
                Capacity = 10,
                StartTime = DateTime.UtcNow - TimeSpan.FromHours(2),
                TotalAmount = 80000,
            },
        };

        SummarizedRentedCardViewModels = new ObservableCollection<SummarizedRentedCardViewModel>(
            mockRentedCards.Select(rentedCardViewModelFactory.Create));

        SummarizedAreaCardViewModels = new ObservableCollection<ISummarizedAreaCardViewModel>(
        [
            .. SummarizedAvailableCardViewModels.Cast<ISummarizedAreaCardViewModel>(),
            .. SummarizedReservedCardViewModels.Cast<ISummarizedAreaCardViewModel>(),
            .. SummarizedRentedCardViewModels.Cast<ISummarizedAreaCardViewModel>(),
        ]);

        _ownedViewModels =
        [
            .. SummarizedAvailableCardViewModels.Cast<IDisposable>(),
            .. SummarizedReservedCardViewModels.Cast<IDisposable>(),
            .. SummarizedRentedCardViewModels.Cast<IDisposable>(),
        ];

        RefreshLocalizedText();
    }

    public ObservableCollection<SummarizedAvailableCardViewModel> SummarizedAvailableCardViewModels { get; }

    public ObservableCollection<SummarizedReservedCardViewModel> SummarizedReservedCardViewModels { get; }

    public ObservableCollection<SummarizedRentedCardViewModel> SummarizedRentedCardViewModels { get; }

    public ObservableCollection<ISummarizedAreaCardViewModel> SummarizedAreaCardViewModels { get; }

    public IAsyncRelayCommand AddAreaCommand { get; }

    public IAsyncRelayCommand FilterAreaCommand { get; }

    public IAsyncRelayCommand AllAreasCommand { get; }

    public IAsyncRelayCommand TableFilterCommand { get; }

    public IAsyncRelayCommand RoomFilterCommand { get; }

    public IAsyncRelayCommand EditAreaCommand { get; }

    public IAsyncRelayCommand DeleteAreaCommand { get; }

    public IRelayCommand<ISummarizedAreaCardViewModel?> SelectSummarizedAreaCardCommand { get; }

    protected override void RefreshLocalizedText()
    {
        ListHeaderText = LocalizationService.GetString("AreaManagementPageListHeader");
        AddAreaButtonText = LocalizationService.GetString("AreaManagementPageAddAreaButton");
        FilterAreaButtonText = LocalizationService.GetString("AreaManagementPageFilterAreaButton");
        AllAreasFilterText = LocalizationService.GetString("AreaManagementPageAllAreasFilter");
        TableFilterText = LocalizationService.GetString("AreaManagementPageTableFilter");
        RoomFilterText = LocalizationService.GetString("AreaManagementPageRoomFilter");
        NoAreasText = LocalizationService.GetString("AreaManagementPageNoAreasText");
    }

    public new void Dispose()
    {
        if (_isDisposed)
            return;

        foreach (var viewModel in _ownedViewModels)
        {
            viewModel.Dispose();
        }

        _isDisposed = true;
        base.Dispose();
    }

    private static Task ExecuteNoopAsync()
    {
        return Task.CompletedTask;
    }

    private void SelectSummarizedAreaCard(ISummarizedAreaCardViewModel? selectedCardViewModel)
    {
        DetailedAreaCardViewModel = CreateDetailedAreaCardViewModel(selectedCardViewModel);
        IsAnySummarizedCardSelected = DetailedAreaCardViewModel is not null;
    }

    private static object? CreateDetailedAreaCardViewModel(ISummarizedAreaCardViewModel? selectedCardViewModel)
    {
        return selectedCardViewModel switch
        {
            SummarizedAvailableCardViewModel => new DetailedAvailableCardViewModel(),
            SummarizedReservedCardViewModel => new DetailedReservedCardViewModel(),
            SummarizedRentedCardViewModel => new DetailedRentedCardViewModel(),
            _ => null,
        };
    }
}
