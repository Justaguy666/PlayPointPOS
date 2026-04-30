using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Application.Areas;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Enums;
using Microsoft.UI.Xaml.Media;
using WinUI.Resources;
using WinUI.Services.Layout;
using WinUI.Services.Factories;
using WinUI.Services.Management;
using WinUI.UIModels;
using WinUI.UIModels.Management;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.Common;
using WinUI.ViewModels.AreaManagement.SummarizedAreaCards;
using WinUI.ViewModels.Dialogs.Management;
using Application.Services.Areas;

namespace WinUI.ViewModels.Pages;

public partial class AreaManagementPageViewModel : LocalizedViewModelBase
{
    private const int PreferredAreaCardsPerRow = 3;
    private const double MinimumSingleColumnWidth = 240;
    private const double MinimumTwoColumnWidth = 420;
    private const double AreaCardOuterHeight = 168;
    private const double AreaCardsColumnSpacing = 8;

    private readonly AreaManagementDialogCoordinator _dialogs;
    private readonly IAreaFilterService _areaFilterService;
    private readonly IResponsiveLayoutService _responsiveLayoutService;
    private readonly AreaDraftFactory _draftFactory;
    private readonly AreaManagementCardViewModelFactory _areaCardViewModelFactory;
    private readonly Brush _selectedFilterBackgroundBrush;
    private readonly Brush _selectedFilterForegroundBrush;
    private readonly Brush _unselectedFilterBackgroundBrush;
    private readonly Brush _unselectedFilterForegroundBrush;
    private readonly List<AreaModel> _allAreaModels;
    private readonly List<ISummarizedAreaCardViewModel> _allSummarizedAreaCardViewModels;
    private readonly ManagementQueryState<AreaFilterCriteria, PlayAreaType?> _queryState;
    private ISummarizedAreaCardViewModel? _selectedSummarizedAreaCardViewModel;
    private IDisposable? _detailedAreaCardDisposable;
    private bool _isDisposed;

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

    public bool IsAllAreasFilterSelected => _queryState.Sort is null;

    public bool IsTableFilterSelected => _queryState.Sort == PlayAreaType.Table;

    public bool IsRoomFilterSelected => _queryState.Sort == PlayAreaType.Room;

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
    public partial string EditAreaShortcutText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DeleteAreaMenuText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DeleteAreaShortcutText { get; set; } = string.Empty;

    public bool IsAnySummarizedCardSelected => DetailedAreaCardViewModel is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAnySummarizedCardSelected))]
    public partial object? DetailedAreaCardViewModel { get; set; }

    [ObservableProperty]
    public partial int AreaCardsMaximumRowsOrColumns { get; set; } = PreferredAreaCardsPerRow;

    [ObservableProperty]
    public partial double AreaCardsMinItemWidth { get; set; } = MinimumSingleColumnWidth;

    [ObservableProperty]
    public partial double AreaCardsMinItemHeight { get; set; } = AreaCardOuterHeight;

    public AreaManagementPageViewModel(
        ILocalizationService localizationService,
        AreaManagementDialogCoordinator dialogs,
        IAreaCatalogService areaCatalogService,
        IAreaFilterService areaFilterService,
        IResponsiveLayoutService responsiveLayoutService,
        AreaModelFactory areaModelFactory,
        AreaDraftFactory draftFactory,
        AreaManagementCardViewModelFactory areaCardViewModelFactory)
        : base(localizationService)
    {
        _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        _areaFilterService = areaFilterService ?? throw new ArgumentNullException(nameof(areaFilterService));
        _responsiveLayoutService = responsiveLayoutService ?? throw new ArgumentNullException(nameof(responsiveLayoutService));
        _draftFactory = draftFactory ?? throw new ArgumentNullException(nameof(draftFactory));
        _areaCardViewModelFactory = areaCardViewModelFactory ?? throw new ArgumentNullException(nameof(areaCardViewModelFactory));
        ArgumentNullException.ThrowIfNull(areaCatalogService);
        ArgumentNullException.ThrowIfNull(areaModelFactory);

        _selectedFilterBackgroundBrush = AppResourceLookup.GetBrush("OrangeFocusBrush", AppColors.OrangeFocus);
        _selectedFilterForegroundBrush = AppResourceLookup.GetBrush("WhiteBrush", AppColors.White);
        _unselectedFilterBackgroundBrush = AppResourceLookup.GetBrush("TransparentBrush", AppColors.Transparent);
        _unselectedFilterForegroundBrush = AppResourceLookup.GetBrush("BlackBrush", AppColors.Black);
        _queryState = new ManagementQueryState<AreaFilterCriteria, PlayAreaType?>(
            new AreaFilterCriteria(),
            null);

        AddAreaCommand = new AsyncRelayCommand(ExecuteAddAreaAsync);
        FilterAreaCommand = new AsyncRelayCommand(OpenAreaFilterDialogAsync);
        AllAreasCommand = new AsyncRelayCommand(ApplyAllAreasFilterAsync);
        TableFilterCommand = new AsyncRelayCommand(ApplyTableFilterAsync);
        RoomFilterCommand = new AsyncRelayCommand(ApplyRoomFilterAsync);
        EditAreaCommand = new AsyncRelayCommand<ISummarizedAreaCardViewModel?>(ExecuteEditAreaAsync);
        DeleteAreaCommand = new AsyncRelayCommand<ISummarizedAreaCardViewModel?>(ExecuteDeleteAreaAsync);
        EditSelectedAreaCommand = new AsyncRelayCommand(ExecuteEditSelectedAreaAsync, CanManageSelectedArea);
        DeleteSelectedAreaCommand = new AsyncRelayCommand(ExecuteDeleteSelectedAreaAsync, CanManageSelectedArea);
        SelectSummarizedAreaCardCommand = new RelayCommand<ISummarizedAreaCardViewModel?>(SelectSummarizedAreaCard);

        _allAreaModels = areaCatalogService
            .GetAreas()
            .Select(areaModelFactory.Create)
            .ToList();
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

    public IAsyncRelayCommand EditSelectedAreaCommand { get; }

    public IAsyncRelayCommand DeleteSelectedAreaCommand { get; }

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
        EditAreaShortcutText = LocalizationService.GetString("AreaManagementPageEditShortcutText");
        DeleteAreaMenuText = LocalizationService.GetString("AreaManagementPageDeleteMenuText");
        DeleteAreaShortcutText = LocalizationService.GetString("AreaManagementPageDeleteShortcutText");
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

    private Task OpenAreaFilterDialogAsync()
    {
        return _dialogs.OpenFilterAsync(_queryState.Filter, HandleAreaFilterSubmittedAsync);
    }

    private Task ExecuteAddAreaAsync()
    {
        return _dialogs.OpenAddAsync(_draftFactory.Create(), HandleAreaCreatedAsync);
    }

    private Task ExecuteEditAreaAsync(ISummarizedAreaCardViewModel? areaCardViewModel)
    {
        var selectedAreaCardViewModel = areaCardViewModel ?? _selectedSummarizedAreaCardViewModel;
        SelectSummarizedAreaCard(selectedAreaCardViewModel);

        if (selectedAreaCardViewModel is null)
        {
            return Task.CompletedTask;
        }

        return _dialogs.OpenEditAsync(selectedAreaCardViewModel.Model, HandleAreaUpdatedAsync);
    }
 
    private Task ExecuteDeleteAreaAsync(ISummarizedAreaCardViewModel? areaCardViewModel)
    {
        return DeleteAreaAsync(areaCardViewModel ?? _selectedSummarizedAreaCardViewModel);
    }

    private Task ExecuteEditSelectedAreaAsync()
    {
        return ExecuteEditAreaAsync(_selectedSummarizedAreaCardViewModel);
    }

    private Task ExecuteDeleteSelectedAreaAsync()
    {
        return DeleteAreaAsync(_selectedSummarizedAreaCardViewModel);
    }

    private async Task DeleteAreaAsync(ISummarizedAreaCardViewModel? areaCardViewModel)
    {
        if (areaCardViewModel is null || areaCardViewModel.Status != PlayAreaStatus.Available)
        {
            return;
        }

        SelectSummarizedAreaCard(areaCardViewModel);

        if (!await _dialogs.ConfirmDeleteAsync())
        {
            return;
        }

        RemoveArea(areaCardViewModel);
        await _dialogs.NotifyDeletedAsync(areaCardViewModel.Model);
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
        OnPropertyChanged(nameof(SelectedSummarizedAreaCardViewModel));
        ReplaceDetailedAreaCardViewModel(_areaCardViewModelFactory.CreateDetailed(selectedCardViewModel));
        NotifySelectedAreaCommandStateChanged();
    }

    public void UpdateAreaCardsLayout(double availableWidth)
    {
        if (availableWidth <= 0)
        {
            return;
        }

        CardGridLayout layout = _responsiveLayoutService.CalculateCardGrid(
            availableWidth,
            MinimumSingleColumnWidth,
            AreaCardOuterHeight,
            AreaCardsColumnSpacing,
            PreferredAreaCardsPerRow,
            wideRows: 1,
            compactPageSize: 1);

        AreaCardsMaximumRowsOrColumns = layout.Columns;
        AreaCardsMinItemWidth = layout.ItemWidth;
        AreaCardsMinItemHeight = AreaCardOuterHeight;
    }

    private void ApplyAreaFilter(PlayAreaType? playAreaType)
    {
        _queryState.Sort = playAreaType;
        AreaFilterCriteria criteria = _queryState.Filter;

        PlayAreaFilter filter = new()
        {
            AreaType = playAreaType,
            Status = criteria.Status,
            StartTimeFrom = criteria.StartTimeFrom,
            StartTimeTo = criteria.StartTimeTo,
            CapacityMin = criteria.CapacityMin,
            CapacityMax = criteria.CapacityMax,
            HourlyPriceMin = criteria.HourlyPriceMin,
            HourlyPriceMax = criteria.HourlyPriceMax,
        };

        var filteredAreas = _areaFilterService.Apply(_allAreaModels, filter, LocalizationService.TimeZone);
        var filteredAreaLookup = filteredAreas.ToHashSet();
        var filteredAreaCards = _allSummarizedAreaCardViewModels
            .Where(card => filteredAreaLookup.Contains(card.Model));

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
        OnPropertyChanged(nameof(SelectedSummarizedAreaCardViewModel));
        ReplaceDetailedAreaCardViewModel(null);
        NotifySelectedAreaCommandStateChanged();
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
        bool isAllAreasSelected = _queryState.Sort is null;
        bool isTableSelected = _queryState.Sort == PlayAreaType.Table;
        bool isRoomSelected = _queryState.Sort == PlayAreaType.Room;

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

    private void ReplaceDetailedAreaCardViewModel(object? detailedAreaCardViewModel)
    {
        _detailedAreaCardDisposable?.Dispose();
        _detailedAreaCardDisposable = detailedAreaCardViewModel as IDisposable;
        DetailedAreaCardViewModel = detailedAreaCardViewModel;
    }

    private bool CanManageSelectedArea()
    {
        return _selectedSummarizedAreaCardViewModel?.Status == PlayAreaStatus.Available;
    }

    private void NotifySelectedAreaCommandStateChanged()
    {
        EditSelectedAreaCommand.NotifyCanExecuteChanged();
        DeleteSelectedAreaCommand.NotifyCanExecuteChanged();
    }

    private void RemoveArea(ISummarizedAreaCardViewModel areaCardViewModel)
    {
        AreaModel area = areaCardViewModel.Model;
        bool wasSelected = ReferenceEquals(_selectedSummarizedAreaCardViewModel, areaCardViewModel)
            || ReferenceEquals(_selectedSummarizedAreaCardViewModel?.Model, area);

        _allAreaModels.Remove(area);
        UnsubscribeFromAreaModel(area);
        _allSummarizedAreaCardViewModels.Remove(areaCardViewModel);

        if (wasSelected)
        {
            SelectSummarizedAreaCard(null);
        }

        ApplyAreaFilter(_queryState.Sort);

        if (areaCardViewModel is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private Task HandleAreaCreatedAsync(AreaModel area)
    {
        _allAreaModels.Add(area);
        SubscribeToAreaModel(area);

        var summarizedAreaCardViewModel = CreateSummarizedAreaCardViewModel(area);
        _allSummarizedAreaCardViewModels.Add(summarizedAreaCardViewModel);

        ApplyAreaFilter(_queryState.Sort);

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
            OnPropertyChanged(nameof(SelectedSummarizedAreaCardViewModel));
        }

        ApplyAreaFilter(_queryState.Sort);

        if (wasSelected)
        {
            ReplaceDetailedAreaCardViewModel(_areaCardViewModelFactory.CreateDetailed(replacementViewModel));
            NotifySelectedAreaCommandStateChanged();
        }

        if (previousViewModel is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private Task HandleAreaUpdatedAsync(AreaModel area)
    {
        ApplyAreaFilter(_queryState.Sort);

        if (_selectedSummarizedAreaCardViewModel?.Model == area)
        {
            ReplaceDetailedAreaCardViewModel(_areaCardViewModelFactory.CreateDetailed(_selectedSummarizedAreaCardViewModel));
        }

        NotifySelectedAreaCommandStateChanged();

        return Task.CompletedTask;
    }

    private Task HandleAreaFilterSubmittedAsync(AreaFilterCriteria criteria)
    {
        _queryState.Filter = criteria;
        ApplyAreaFilter(_queryState.Sort);
        return Task.CompletedTask;
    }

    private ISummarizedAreaCardViewModel CreateSummarizedAreaCardViewModel(AreaModel area)
    {
        return _areaCardViewModelFactory.CreateSummarized(area);
    }
}
