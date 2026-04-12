using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Enums;
using Microsoft.UI.Xaml.Media;
using WinUI.Resources;
using WinUI.Services.Factories;
using WinUI.UIModels;
using WinUI.UIModels.AreaManagement;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.AreaManagement.DetailedAreaCards;
using WinUI.ViewModels.AreaManagement.SummarizedAreaCards;
using WinUI.ViewModels.Dialogs.Management;

namespace WinUI.ViewModels.Pages;

public partial class AreaManagementPageViewModel : LocalizedViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly ILocalizationPreferencesService _localizationPreferencesService;
    private readonly SummarizedAvailableCardViewModelFactory _availableCardViewModelFactory;
    private readonly SummarizedReservedCardViewModelFactory _reservedCardViewModelFactory;
    private readonly SummarizedRentedCardViewModelFactory _rentedCardViewModelFactory;
    private readonly Brush _selectedFilterBackgroundBrush;
    private readonly Brush _selectedFilterForegroundBrush;
    private readonly Brush _unselectedFilterBackgroundBrush;
    private readonly Brush _unselectedFilterForegroundBrush;
    private readonly List<AreaModel> _allAreaModels;
    private readonly List<ISummarizedAreaCardViewModel> _allSummarizedAreaCardViewModels;
    private ISummarizedAreaCardViewModel? _selectedSummarizedAreaCardViewModel;
    private IDisposable? _detailedAreaCardDisposable;
    private bool _isDisposed;
    private PlayAreaType? _activeAreaFilterType;
    private PlayAreaStatus? _activeStatusFilter;
    private TimeSpan? _activeStartTimeFromFilter;
    private TimeSpan? _activeStartTimeToFilter;
    private int? _activeCapacityMinFilter;
    private int? _activeCapacityMaxFilter;
    private decimal? _activeHourlyPriceMinFilter;
    private decimal? _activeHourlyPriceMaxFilter;

    [ObservableProperty]
    public partial string ListHeaderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AddAreaButtonText { get; set; } = string.Empty;

    public IconState AddIconState { get; } = new()
    {
        Kind = IconKind.Add,
        Size = 24,
        AlwaysFilled = true,
    };

    [ObservableProperty]
    public partial string FilterAreaButtonText { get; set; } = string.Empty;

    public IconState FilterIconState { get; } = new()
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
    public partial Brush AllAreasFilterBackgroundBrush { get; set; } = new SolidColorBrush(AppColors.Transparent);

    [ObservableProperty]
    public partial Brush AllAreasFilterForegroundBrush { get; set; } = new SolidColorBrush(AppColors.Black);

    [ObservableProperty]
    public partial Brush TableFilterBackgroundBrush { get; set; } = new SolidColorBrush(AppColors.Transparent);

    [ObservableProperty]
    public partial Brush TableFilterForegroundBrush { get; set; } = new SolidColorBrush(AppColors.Black);

    [ObservableProperty]
    public partial Brush RoomFilterBackgroundBrush { get; set; } = new SolidColorBrush(AppColors.Transparent);

    [ObservableProperty]
    public partial Brush RoomFilterForegroundBrush { get; set; } = new SolidColorBrush(AppColors.Black);

    public bool IsAllAreasFilterSelected => _activeAreaFilterType is null;

    public bool IsTableFilterSelected => _activeAreaFilterType == PlayAreaType.Table;

    public bool IsRoomFilterSelected => _activeAreaFilterType == PlayAreaType.Room;

    public IconState NoAreasIconState { get; } = new()
    {
        Kind = IconKind.Dice,
        AlwaysFilled = true,
        Size = 48,
    };

    [ObservableProperty]
    public partial string NoAreasText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EditAreaMenuText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DeleteAreaMenuText { get; set; } = string.Empty;

    public bool IsAnySummarizedCardSelected => DetailedAreaCardViewModel is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAnySummarizedCardSelected))]
    public partial object? DetailedAreaCardViewModel { get; set; }

    public AreaManagementPageViewModel(
        ILocalizationService localizationService,
        ILocalizationPreferencesService localizationPreferencesService,
        IDialogService dialogService,
        SummarizedAvailableCardViewModelFactory availableCardViewModelFactory,
        SummarizedReservedCardViewModelFactory reservedCardViewModelFactory,
        SummarizedRentedCardViewModelFactory rentedCardViewModelFactory)
        : base(localizationService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _localizationPreferencesService = localizationPreferencesService ?? throw new ArgumentNullException(nameof(localizationPreferencesService));
        ArgumentNullException.ThrowIfNull(availableCardViewModelFactory);
        ArgumentNullException.ThrowIfNull(reservedCardViewModelFactory);
        ArgumentNullException.ThrowIfNull(rentedCardViewModelFactory);
        _availableCardViewModelFactory = availableCardViewModelFactory;
        _reservedCardViewModelFactory = reservedCardViewModelFactory;
        _rentedCardViewModelFactory = rentedCardViewModelFactory;

        _selectedFilterBackgroundBrush = AppResourceLookup.GetBrush("OrangeFocusBrush", AppColors.OrangeFocus);
        _selectedFilterForegroundBrush = AppResourceLookup.GetBrush("WhiteBrush", AppColors.White);
        _unselectedFilterBackgroundBrush = AppResourceLookup.GetBrush("TransparentBrush", AppColors.Transparent);
        _unselectedFilterForegroundBrush = AppResourceLookup.GetBrush("BlackBrush", AppColors.Black);

        AddAreaCommand = new AsyncRelayCommand(ExecuteAddAreaAsync);
        FilterAreaCommand = new AsyncRelayCommand(OpenAreaFilterDialogAsync);
        AllAreasCommand = new AsyncRelayCommand(ApplyAllAreasFilterAsync);
        TableFilterCommand = new AsyncRelayCommand(ApplyTableFilterAsync);
        RoomFilterCommand = new AsyncRelayCommand(ApplyRoomFilterAsync);
        EditAreaCommand = new AsyncRelayCommand<ISummarizedAreaCardViewModel?>(ExecuteEditAreaAsync);
        DeleteAreaCommand = new AsyncRelayCommand<ISummarizedAreaCardViewModel?>(ExecuteDeleteAreaAsync);
        SelectSummarizedAreaCardCommand = new RelayCommand<ISummarizedAreaCardViewModel?>(SelectSummarizedAreaCard);

        _allAreaModels = CreateMockAreaModels();
        foreach (var areaModel in _allAreaModels)
        {
            SubscribeToAreaModel(areaModel);
        }

        _allSummarizedAreaCardViewModels = _allAreaModels
            .Select(CreateSummarizedAreaCardViewModel)
            .ToList();

        SummarizedAreaCardViewModels = new ObservableCollection<ISummarizedAreaCardViewModel>();

        RefreshLocalizedText();
        ApplyAreaFilter(null);
    }

    public ObservableCollection<ISummarizedAreaCardViewModel> SummarizedAreaCardViewModels { get; }

    public ISummarizedAreaCardViewModel? SelectedSummarizedAreaCardViewModel => _selectedSummarizedAreaCardViewModel;

    public IAsyncRelayCommand AddAreaCommand { get; }

    public IAsyncRelayCommand FilterAreaCommand { get; }

    public IAsyncRelayCommand AllAreasCommand { get; }

    public IAsyncRelayCommand TableFilterCommand { get; }

    public IAsyncRelayCommand RoomFilterCommand { get; }

    public IAsyncRelayCommand<ISummarizedAreaCardViewModel?> EditAreaCommand { get; }

    public IAsyncRelayCommand<ISummarizedAreaCardViewModel?> DeleteAreaCommand { get; }

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
        EditAreaMenuText = LocalizationService.GetString("AreaManagementPageEditMenuText");
        DeleteAreaMenuText = LocalizationService.GetString("AreaManagementPageDeleteMenuText");
    }

    public new void Dispose()
    {
        if (_isDisposed)
            return;

        foreach (var viewModel in _allSummarizedAreaCardViewModels.OfType<IDisposable>())
        {
            viewModel.Dispose();
        }

        foreach (var areaModel in _allAreaModels)
        {
            UnsubscribeFromAreaModel(areaModel);
        }

        _detailedAreaCardDisposable?.Dispose();
        _detailedAreaCardDisposable = null;
        _isDisposed = true;
        base.Dispose();
    }

    private static Task ExecuteNoopAsync()
    {
        return Task.CompletedTask;
    }

    private Task OpenAreaFilterDialogAsync()
    {
        return _dialogService.ShowDialogAsync(
            "AreaFilter",
            new AreaFilterDialogRequest
            {
                InitialCriteria = new AreaFilterCriteria
                {
                    Status = _activeStatusFilter,
                    StartTimeFrom = _activeStartTimeFromFilter,
                    StartTimeTo = _activeStartTimeToFilter,
                    CapacityMin = _activeCapacityMinFilter,
                    CapacityMax = _activeCapacityMaxFilter,
                    HourlyPriceMin = _activeHourlyPriceMinFilter,
                    HourlyPriceMax = _activeHourlyPriceMaxFilter,
                },
                OnSubmittedAsync = HandleAreaFilterSubmittedAsync,
            });
    }

    private Task ExecuteAddAreaAsync()
    {
        var area = new AreaModel
        {
            Status = PlayAreaStatus.Available,
            PlayAreaType = PlayAreaType.Table,
            MaxCapacity = 2,
            HourlyPrice = 30000m,
        };

        return _dialogService.ShowDialogAsync(
            "Area",
            new AreaDialogRequest
            {
                Mode = UpsertDialogMode.Add,
                Model = area,
                OnSubmittedAsync = HandleAreaCreatedAsync,
            });
    }

    private Task ExecuteEditAreaAsync(ISummarizedAreaCardViewModel? areaCardViewModel)
    {
        var selectedAreaCardViewModel = areaCardViewModel ?? _selectedSummarizedAreaCardViewModel;
        SelectSummarizedAreaCard(selectedAreaCardViewModel);

        if (selectedAreaCardViewModel is null)
        {
            return Task.CompletedTask;
        }

        return _dialogService.ShowDialogAsync(
            "Area",
            new AreaDialogRequest
            {
                Mode = UpsertDialogMode.Edit,
                Model = selectedAreaCardViewModel.Model,
                OnSubmittedAsync = HandleAreaUpdatedAsync,
            });
    }
 
    private Task ExecuteDeleteAreaAsync(ISummarizedAreaCardViewModel? areaCardViewModel)
    {
        SelectSummarizedAreaCard(areaCardViewModel ?? _selectedSummarizedAreaCardViewModel);
        return Task.CompletedTask;
    }

    private Task ApplyAllAreasFilterAsync()
    {
        ApplyAreaFilter(null);
        return Task.CompletedTask;
    }

    private Task ApplyTableFilterAsync()
    {
        ApplyAreaFilter(PlayAreaType.Table);
        return Task.CompletedTask;
    }

    private Task ApplyRoomFilterAsync()
    {
        ApplyAreaFilter(PlayAreaType.Room);
        return Task.CompletedTask;
    }

    private void SelectSummarizedAreaCard(ISummarizedAreaCardViewModel? selectedCardViewModel)
    {
        _selectedSummarizedAreaCardViewModel = selectedCardViewModel;
        ReplaceDetailedAreaCardViewModel(CreateDetailedAreaCardViewModel(selectedCardViewModel));
    }

    private void ApplyAreaFilter(PlayAreaType? playAreaType)
    {
        _activeAreaFilterType = playAreaType;

        var filteredAreaCards = _allSummarizedAreaCardViewModels
            .Where(card =>
                (playAreaType is null || card.PlayAreaType == playAreaType) &&
                (_activeStatusFilter is null || card.Status == _activeStatusFilter) &&
                MatchesStartTimeFilter(card.Model) &&
                MatchesCapacityFilter(card.Model) &&
                MatchesHourlyPriceFilter(card.Model));

        ReplaceSummarizedAreaCardViewModels(filteredAreaCards);
        RefreshAreaFilterButtonState();

        if (_selectedSummarizedAreaCardViewModel is null)
        {
            return;
        }

        if (SummarizedAreaCardViewModels.Contains(_selectedSummarizedAreaCardViewModel))
        {
            return;
        }

        _selectedSummarizedAreaCardViewModel = null;
        ReplaceDetailedAreaCardViewModel(null);
    }
 
    private void ReplaceSummarizedAreaCardViewModels(IEnumerable<ISummarizedAreaCardViewModel> filteredAreaCards)
    {
        SummarizedAreaCardViewModels.Clear();

        foreach (var areaCardViewModel in filteredAreaCards)
        {
            SummarizedAreaCardViewModels.Add(areaCardViewModel);
        }
    }

    private void RefreshAreaFilterButtonState()
    {
        bool isAllAreasSelected = _activeAreaFilterType is null;
        bool isTableSelected = _activeAreaFilterType == PlayAreaType.Table;
        bool isRoomSelected = _activeAreaFilterType == PlayAreaType.Room;

        AllAreasFilterBackgroundBrush = isAllAreasSelected ? _selectedFilterBackgroundBrush : _unselectedFilterBackgroundBrush;
        AllAreasFilterForegroundBrush = isAllAreasSelected ? _selectedFilterForegroundBrush : _unselectedFilterForegroundBrush;

        TableFilterBackgroundBrush = isTableSelected ? _selectedFilterBackgroundBrush : _unselectedFilterBackgroundBrush;
        TableFilterForegroundBrush = isTableSelected ? _selectedFilterForegroundBrush : _unselectedFilterForegroundBrush;

        RoomFilterBackgroundBrush = isRoomSelected ? _selectedFilterBackgroundBrush : _unselectedFilterBackgroundBrush;
        RoomFilterForegroundBrush = isRoomSelected ? _selectedFilterForegroundBrush : _unselectedFilterForegroundBrush;

        TableFilterIconState = CreateFilterIconState(IconKind.Table, isTableSelected);
        RoomFilterIconState = CreateFilterIconState(IconKind.Room, isRoomSelected);

        OnPropertyChanged(nameof(IsAllAreasFilterSelected));
        OnPropertyChanged(nameof(IsTableFilterSelected));
        OnPropertyChanged(nameof(IsRoomFilterSelected));
    }

    private static IconState CreateFilterIconState(IconKind iconKind, bool isSelected)
    {
        return new IconState
        {
            Kind = iconKind,
            Size = 24,
            IsSelected = isSelected,
        };
    }

    private object? CreateDetailedAreaCardViewModel(ISummarizedAreaCardViewModel? selectedCardViewModel)
    {
        return selectedCardViewModel switch
        {
            SummarizedAvailableCardViewModel availableCardViewModel => new DetailedAvailableCardViewModel(
                LocalizationService,
                _dialogService,
                availableCardViewModel.Model),
            SummarizedReservedCardViewModel reservedCardViewModel => new DetailedReservedCardViewModel(
                LocalizationService,
                _localizationPreferencesService,
                _dialogService,
                reservedCardViewModel.Model),
            SummarizedRentedCardViewModel rentedCardViewModel => new DetailedRentedCardViewModel(
                LocalizationService,
                _dialogService,
                rentedCardViewModel.Model),
            _ => null,
        };
    }

    private void ReplaceDetailedAreaCardViewModel(object? detailedAreaCardViewModel)
    {
        _detailedAreaCardDisposable?.Dispose();
        _detailedAreaCardDisposable = detailedAreaCardViewModel as IDisposable;
        DetailedAreaCardViewModel = detailedAreaCardViewModel;
    }

    private Task HandleAreaCreatedAsync(AreaModel area)
    {
        _allAreaModels.Add(area);
        SubscribeToAreaModel(area);

        var summarizedAreaCardViewModel = CreateSummarizedAreaCardViewModel(area);
        _allSummarizedAreaCardViewModels.Add(summarizedAreaCardViewModel);

        ApplyAreaFilter(_activeAreaFilterType);

        if (SummarizedAreaCardViewModels.Contains(summarizedAreaCardViewModel))
        {
            SelectSummarizedAreaCard(summarizedAreaCardViewModel);
        }

        return Task.CompletedTask;
    }

    private void SubscribeToAreaModel(AreaModel area)
    {
        area.PropertyChanged += HandleAreaModelPropertyChanged;
    }

    private void UnsubscribeFromAreaModel(AreaModel area)
    {
        area.PropertyChanged -= HandleAreaModelPropertyChanged;
    }

    private void HandleAreaModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isDisposed ||
            sender is not AreaModel area ||
            e.PropertyName != nameof(AreaModel.Status))
        {
            return;
        }

        RefreshAreaCardViewModelsForModel(area);
    }

    private void RefreshAreaCardViewModelsForModel(AreaModel area)
    {
        int index = _allSummarizedAreaCardViewModels.FindIndex(viewModel => ReferenceEquals(viewModel.Model, area));
        if (index < 0)
        {
            return;
        }

        ISummarizedAreaCardViewModel previousViewModel = _allSummarizedAreaCardViewModels[index];
        ISummarizedAreaCardViewModel replacementViewModel = CreateSummarizedAreaCardViewModel(area);
        bool wasSelected = ReferenceEquals(_selectedSummarizedAreaCardViewModel?.Model, area);

        _allSummarizedAreaCardViewModels[index] = replacementViewModel;

        if (wasSelected)
        {
            _selectedSummarizedAreaCardViewModel = replacementViewModel;
        }

        ApplyAreaFilter(_activeAreaFilterType);

        if (wasSelected)
        {
            ReplaceDetailedAreaCardViewModel(CreateDetailedAreaCardViewModel(replacementViewModel));
        }

        if (previousViewModel is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private Task HandleAreaUpdatedAsync(AreaModel area)
    {
        ApplyAreaFilter(_activeAreaFilterType);

        if (_selectedSummarizedAreaCardViewModel?.Model == area)
        {
            ReplaceDetailedAreaCardViewModel(CreateDetailedAreaCardViewModel(_selectedSummarizedAreaCardViewModel));
        }

        return Task.CompletedTask;
    }

    private Task HandleAreaFilterSubmittedAsync(AreaFilterCriteria criteria)
    {
        _activeStatusFilter = criteria.Status;
        _activeStartTimeFromFilter = criteria.StartTimeFrom;
        _activeStartTimeToFilter = criteria.StartTimeTo;
        _activeCapacityMinFilter = criteria.CapacityMin;
        _activeCapacityMaxFilter = criteria.CapacityMax;
        _activeHourlyPriceMinFilter = criteria.HourlyPriceMin;
        _activeHourlyPriceMaxFilter = criteria.HourlyPriceMax;
        ApplyAreaFilter(_activeAreaFilterType);
        return Task.CompletedTask;
    }

    private bool MatchesStartTimeFilter(AreaModel area)
    {
        if (_activeStartTimeFromFilter is null && _activeStartTimeToFilter is null)
        {
            return true;
        }

        TimeSpan? areaStartTime = GetComparableStartTime(area);
        if (!areaStartTime.HasValue)
        {
            return false;
        }

        return (_activeStartTimeFromFilter is null || areaStartTime.Value >= _activeStartTimeFromFilter.Value)
            && (_activeStartTimeToFilter is null || areaStartTime.Value <= _activeStartTimeToFilter.Value);
    }

    private bool MatchesCapacityFilter(AreaModel area)
    {
        int comparableCapacity = area.MaxCapacity > 0 ? area.MaxCapacity : area.Capacity;

        return (_activeCapacityMinFilter is null || comparableCapacity >= _activeCapacityMinFilter.Value)
            && (_activeCapacityMaxFilter is null || comparableCapacity <= _activeCapacityMaxFilter.Value);
    }

    private bool MatchesHourlyPriceFilter(AreaModel area)
    {
        return (_activeHourlyPriceMinFilter is null || area.HourlyPrice >= _activeHourlyPriceMinFilter.Value)
            && (_activeHourlyPriceMaxFilter is null || area.HourlyPrice <= _activeHourlyPriceMaxFilter.Value);
    }

    private TimeSpan? GetComparableStartTime(AreaModel area)
    {
        if (area.StartTime is DateTime startTime)
        {
            return ConvertUtcToConfiguredTime(startTime).TimeOfDay;
        }

        if (area.CheckInDateTime is DateTime checkInDateTime)
        {
            return checkInDateTime.TimeOfDay;
        }

        return null;
    }

    private DateTime ConvertUtcToConfiguredTime(DateTime value)
    {
        DateTime utcValue = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        return utcValue.AddHours(ParseTimeZoneOffset(LocalizationService.TimeZone));
    }

    private static int ParseTimeZoneOffset(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        string normalized = value.Trim().ToUpperInvariant().Replace("UTC", string.Empty);
        return int.TryParse(normalized, out int offset) ? offset : 0;
    }

    private ISummarizedAreaCardViewModel CreateSummarizedAreaCardViewModel(AreaModel area)
    {
        return area.Status switch
        {
            PlayAreaStatus.Available => _availableCardViewModelFactory.Create(area),
            PlayAreaStatus.Reserved => _reservedCardViewModelFactory.Create(area),
            PlayAreaStatus.Rented => _rentedCardViewModelFactory.Create(area),
            _ => _availableCardViewModelFactory.Create(area),
        };
    }

    private static List<AreaModel> CreateMockAreaModels()
    {
        return
        [
            new AreaModel
            {
                AreaName = "Ban A01",
                PlayAreaType = PlayAreaType.Table,
                Status = PlayAreaStatus.Available,
                MaxCapacity = 4,
                HourlyPrice = 30000m,
            },
            new AreaModel
            {
                AreaName = "Ban A02",
                PlayAreaType = PlayAreaType.Table,
                Status = PlayAreaStatus.Available,
                MaxCapacity = 4,
                HourlyPrice = 30000m,
            },
            new AreaModel
            {
                AreaName = "Ban B01",
                PlayAreaType = PlayAreaType.Table,
                Status = PlayAreaStatus.Available,
                MaxCapacity = 6,
                HourlyPrice = 45000m,
            },
            new AreaModel
            {
                AreaName = "Phong VIP 01",
                PlayAreaType = PlayAreaType.Room,
                Status = PlayAreaStatus.Available,
                MaxCapacity = 8,
                HourlyPrice = 80000m,
            },
            new AreaModel
            {
                AreaName = "Phong VIP 02",
                PlayAreaType = PlayAreaType.Room,
                Status = PlayAreaStatus.Available,
                MaxCapacity = 10,
                HourlyPrice = 100000m,
            },
            new AreaModel
            {
                AreaName = "Ban S05",
                PlayAreaType = PlayAreaType.Table,
                Status = PlayAreaStatus.Available,
                MaxCapacity = 4,
                HourlyPrice = 35000m,
            },
            new AreaModel
            {
                AreaName = "Phong R01",
                PlayAreaType = PlayAreaType.Room,
                Status = PlayAreaStatus.Reserved,
                MaxCapacity = 8,
                CustomerName = "Tran Minh Anh",
                PhoneNumber = "0789 608 537",
                CheckInDateTime = DateTime.Today.AddHours(13).AddMinutes(30),
                Capacity = 6,
                HourlyPrice = 80000m,
            },
            new AreaModel
            {
                AreaName = "Ban C02",
                PlayAreaType = PlayAreaType.Table,
                Status = PlayAreaStatus.Reserved,
                MaxCapacity = 4,
                CustomerName = "Nguyen Hoang Long",
                PhoneNumber = "0789 608 537",
                CheckInDateTime = DateTime.Today.AddHours(15),
                Capacity = 4,
                HourlyPrice = 40000m,
            },
            new AreaModel
            {
                AreaName = "Phong VIP 03",
                PlayAreaType = PlayAreaType.Room,
                Status = PlayAreaStatus.Reserved,
                MaxCapacity = 10,
                CustomerName = "Le Thu Ha",
                PhoneNumber = "0789 608 537",
                CheckInDateTime = DateTime.Today.AddHours(18).AddMinutes(15),
                Capacity = 8,
                HourlyPrice = 100000m,
            },
            new AreaModel
            {
                AreaName = "Ban K04 (Future)",
                PlayAreaType = PlayAreaType.Table,
                Status = PlayAreaStatus.Reserved,
                MaxCapacity = 6,
                CustomerName = "Ngo Bao Toan",
                PhoneNumber = "0123 456 789",
                CheckInDateTime = DateTime.Now.AddHours(2),
                Capacity = 5,
                HourlyPrice = 50000m,
            },
            new AreaModel
            {
                AreaName = "Phong 01",
                PlayAreaType = PlayAreaType.Room,
                Status = PlayAreaStatus.Rented,
                MaxCapacity = 8,
                Capacity = 6,
                StartTime = DateTime.UtcNow - new TimeSpan(1, 31, 34),
                HourlyPrice = 10000m,
                TotalAmount = 110000m,
            },
            new AreaModel
            {
                AreaName = "Ban D02",
                PlayAreaType = PlayAreaType.Table,
                Status = PlayAreaStatus.Rented,
                MaxCapacity = 4,
                Capacity = 3,
                StartTime = DateTime.UtcNow - TimeSpan.FromMinutes(52),
                HourlyPrice = 30000m,
                TotalAmount = 85000m,
            },
            new AreaModel
            {
                AreaName = "Phong VIP 04",
                PlayAreaType = PlayAreaType.Room,
                Status = PlayAreaStatus.Rented,
                MaxCapacity = 10,
                Capacity = 8,
                StartTime = DateTime.UtcNow - TimeSpan.FromHours(2),
                HourlyPrice = 40000m,
                TotalAmount = 205000m,
            },
        ];
    }
}
