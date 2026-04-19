using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Application.Members;
using Application.Services;
using Application.Services.Members;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Entities;
using WinUI.Resources;
using WinUI.Services.Factories;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;
using WinUI.ViewModels.Dialogs.Management;
using WinUI.ViewModels.UserControls;
using WinUI.ViewModels.UserControls.Members;

namespace WinUI.ViewModels.Pages;

public partial class MemberManagementPageViewModel : LocalizedViewModelBase
{
    private const int PageSize = 7;
    private const string SortFieldName = "name";
    private const string SortFieldSpent = "spent";
    private const string SortFieldPackage = "package";
    private const string SortFieldProgress = "progress";
    private const string SortAscending = "asc";
    private const string SortDescending = "desc";

    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;
    private readonly IMemberFilterService _memberFilterService;
    private readonly MemberModelFactory _memberModelFactory;
    private readonly MemberCardControlViewModelFactory _memberCardControlViewModelFactory;
    private readonly List<MemberModel> _allMembers;
    private readonly List<MembershipRank> _allMembershipRanks;
    private readonly Dictionary<MemberModel, MemberCardControlViewModel> _cardViewModelsByMember;
    private readonly PaginationModel _pagination;
    private IReadOnlyList<MemberModel> _filteredMembers = [];
    private MembershipRank? _activeMembershipRankFilter;
    private decimal? _activeTotalSpentMinFilter;
    private decimal? _activeTotalSpentMaxFilter;
    private bool _isUpdatingSelectionOptions;
    private bool _isInitialized;
    private bool _isDisposed;

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SearchPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SortFieldPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SortDirectionPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AddMemberButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ManageMembershipPackagesButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FilterButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PageInfoText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NoMembersText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? SelectedSortField { get; set; } = SortFieldName;

    [ObservableProperty]
    public partial string? SelectedSortDirection { get; set; } = SortAscending;

    [ObservableProperty]
    public partial IconState FilterIconState { get; set; } = new() { Kind = IconKind.Filter, Size = 20, AlwaysFilled = false };

    [ObservableProperty]
    public partial IconState AddIconState { get; set; } = new() { Kind = IconKind.Add, Size = 20, AlwaysFilled = true };

    public IconState NoMembersIconState { get; } = new()
    {
        Kind = IconKind.Member,
        Size = 52,
        AlwaysFilled = true,
    };

    public ObservableCollection<MemberCardControlViewModel> PagedMemberCards { get; } = [];

    public ObservableCollection<LocalizationOptionModel> SortFieldOptions { get; } = [];

    public ObservableCollection<LocalizationOptionModel> SortDirectionOptions { get; } = [];

    public PaginationControlViewModel PaginationViewModel { get; }

    public PaginationModel Pagination => _pagination;

    public IconKind SearchIconKind => IconKind.Search;

    public bool HasMembers => _filteredMembers.Count > 0;

    public IAsyncRelayCommand FilterCommand { get; }

    public IAsyncRelayCommand AddMemberCommand { get; }

    public IAsyncRelayCommand ManageMembershipPackagesCommand { get; }

    public IRelayCommand ClearSearchCommand { get; }

    public MemberManagementPageViewModel(
        ILocalizationService localizationService,
        IDialogService dialogService,
        INotificationService notificationService,
        IMemberCatalogService memberCatalogService,
        IMembershipRankCatalogService membershipRankCatalogService,
        IMemberFilterService memberFilterService,
        MemberModelFactory memberModelFactory,
        MemberCardControlViewModelFactory memberCardControlViewModelFactory,
        PaginationControlViewModel paginationViewModel)
        : base(localizationService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _memberFilterService = memberFilterService ?? throw new ArgumentNullException(nameof(memberFilterService));
        _memberModelFactory = memberModelFactory ?? throw new ArgumentNullException(nameof(memberModelFactory));
        _memberCardControlViewModelFactory = memberCardControlViewModelFactory ?? throw new ArgumentNullException(nameof(memberCardControlViewModelFactory));
        PaginationViewModel = paginationViewModel ?? throw new ArgumentNullException(nameof(paginationViewModel));
        ArgumentNullException.ThrowIfNull(memberCatalogService);
        ArgumentNullException.ThrowIfNull(membershipRankCatalogService);

        _pagination = new PaginationModel
        {
            CurrentPage = 1,
            PageSize = PageSize,
            MaxVisiblePageButtons = 4,
        };

        PaginationViewModel.Pagination = _pagination;
        _pagination.PropertyChanged += HandlePaginationPropertyChanged;

        FilterCommand = new AsyncRelayCommand(OpenFilterDialogAsync);
        AddMemberCommand = new AsyncRelayCommand(ExecuteAddMemberAsync);
        ManageMembershipPackagesCommand = new AsyncRelayCommand(ExecuteManageMembershipPackagesAsync);
        ClearSearchCommand = new RelayCommand(ClearSearch);

        _allMembershipRanks = membershipRankCatalogService
            .GetMembershipRanks()
            .OrderBy(rank => rank.MinSpentAmount)
            .ThenBy(rank => rank.Priority)
            .ToList();
        NormalizeMembershipRanks();

        _allMembers = memberCatalogService
            .GetMembers()
            .Select(_memberModelFactory.Create)
            .ToList();
        RefreshMembershipStates();

        _cardViewModelsByMember = _allMembers.ToDictionary(member => member, CreateCardViewModel);

        RefreshLocalizedText();
        _isInitialized = true;
        ApplyFiltersAndSorting(resetToFirstPage: true);
    }

    protected override void RefreshLocalizedText()
    {
        AddMemberButtonText = LocalizationService.GetString("MemberManagementPageAddMemberButton");
        ManageMembershipPackagesButtonText = LocalizationService.GetString("MemberManagementPageManageMembershipPackagesButton");
        FilterButtonText = LocalizationService.GetString("MemberManagementPageFilterButton");
        SearchPlaceholderText = LocalizationService.GetString("MemberManagementPageSearchPlaceholderText");
        SortFieldPlaceholderText = LocalizationService.GetString("MemberManagementPageSortFieldComboBox.PlaceholderText");
        SortDirectionPlaceholderText = LocalizationService.GetString("MemberManagementPageSortDirectionComboBox.PlaceholderText");
        NoMembersText = LocalizationService.GetString("MemberManagementPageNoMembersText");

        RefreshSortOptions();
        UpdatePageMetadata();
    }

    public new void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _pagination.PropertyChanged -= HandlePaginationPropertyChanged;
        foreach (MemberCardControlViewModel viewModel in _cardViewModelsByMember.Values)
        {
            viewModel.Dispose();
        }

        PaginationViewModel.Dispose();
        _isDisposed = true;
        base.Dispose();
    }

    partial void OnSearchTextChanged(string value)
    {
        if (!_isInitialized)
        {
            return;
        }

        ApplyFiltersAndSorting(resetToFirstPage: true);
    }

    partial void OnSelectedSortFieldChanged(string? value)
    {
        if (!_isInitialized || _isUpdatingSelectionOptions)
        {
            return;
        }

        ApplyFiltersAndSorting(resetToFirstPage: true);
    }

    partial void OnSelectedSortDirectionChanged(string? value)
    {
        if (!_isInitialized || _isUpdatingSelectionOptions)
        {
            return;
        }

        ApplyFiltersAndSorting(resetToFirstPage: true);
    }

    private void HandlePaginationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PaginationModel.CurrentPage) or
            nameof(PaginationModel.PageSize) or
            nameof(PaginationModel.TotalItems))
        {
            RefreshPagedMembers();
        }
    }

    private async Task OpenFilterDialogAsync()
    {
        await _dialogService.ShowDialogAsync(
            "MemberFilter",
            new MemberFilterDialogRequest
            {
                AvailableMembershipRanks = _allMembershipRanks,
                InitialCriteria = BuildCurrentFilterCriteria(),
                OnSubmittedAsync = HandleFilterSubmittedAsync,
            });
    }

    private async Task ExecuteAddMemberAsync()
    {
        await _dialogService.ShowDialogAsync(
            "Member",
            new MemberDialogRequest
            {
                Mode = UpsertDialogMode.Add,
                Model = CreateNewMemberDraft(),
                AvailableMembershipRanks = _allMembershipRanks,
                OnSubmittedAsync = HandleMemberCreatedAsync,
            });
    }

    private async Task ExecuteManageMembershipPackagesAsync()
    {
        var membershipRanksCollection = new ObservableCollection<MembershipRank>(_allMembershipRanks);

        await _dialogService.ShowDialogAsync(
            "MembershipPackage",
            new MembershipPackageDialogRequest
            {
                MembershipRanks = membershipRanksCollection,
                OnMembershipRankAddedAsync = rank =>
                {
                    if (!_allMembershipRanks.Contains(rank))
                    {
                        _allMembershipRanks.Add(rank);
                    }

                    NormalizeMembershipRanks();
                    RefreshMembershipStates();
                    ApplyFiltersAndSorting(resetToFirstPage: false);
                    return Task.CompletedTask;
                },
                OnMembershipRankDeletedAsync = rank =>
                {
                    _allMembershipRanks.Remove(rank);
                    NormalizeMembershipRanks();
                    RefreshMembershipStates();
                    ApplyFiltersAndSorting(resetToFirstPage: false);
                    return Task.CompletedTask;
                },
                OnMembershipRankUpdatedAsync = rank =>
                {
                    NormalizeMembershipRanks();
                    RefreshMembershipStates();
                    ApplyFiltersAndSorting(resetToFirstPage: false);
                    return Task.CompletedTask;
                },
            });
    }

    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    private Task HandleEditMemberAsync(MemberModel member)
    {
        if (member is null)
        {
            return Task.CompletedTask;
        }

        return _dialogService.ShowDialogAsync(
            "Member",
            new MemberDialogRequest
            {
                Mode = UpsertDialogMode.Edit,
                Model = member,
                AvailableMembershipRanks = _allMembershipRanks,
                OnSubmittedAsync = HandleMemberUpdatedAsync,
            });
    }

    private async Task HandleDeleteMemberAsync(MemberModel member)
    {
        if (member is null)
        {
            return;
        }

        bool isConfirmed = await _dialogService.ShowConfirmationAsync(
            titleKey: "ConfirmDeleteMemberTitle",
            messageKey: "ConfirmDeleteMemberMessage",
            confirmButtonTextKey: "ConfirmDeleteMemberButton",
            cancelButtonTextKey: "CancelButtonText");

        if (!isConfirmed)
        {
            return;
        }

        if (!_allMembers.Remove(member))
        {
            return;
        }

        RemoveCardViewModelForMember(member);
        ApplyFiltersAndSorting(resetToFirstPage: false);

        await _notificationService.SendAsync(
            LocalizationService.GetString("MemberDeletedSuccessTitle"),
            string.Format(
                LocalizationService.Culture,
                LocalizationService.GetString("MemberDeletedSuccessMessage"),
                member.FullName),
            NotificationType.Success);
    }

    private Task HandleFilterSubmittedAsync(MemberFilter criteria)
    {
        _activeMembershipRankFilter = criteria.MembershipRank;
        _activeTotalSpentMinFilter = criteria.TotalSpentMin;
        _activeTotalSpentMaxFilter = criteria.TotalSpentMax;

        ApplyFiltersAndSorting(resetToFirstPage: true);
        return Task.CompletedTask;
    }

    private async Task HandleMemberCreatedAsync(MemberModel member)
    {
        ApplyMembershipState(member);

        if (!_allMembers.Contains(member))
        {
            _allMembers.Insert(0, member);
            AddCardViewModelForMember(member);
        }

        ApplyFiltersAndSorting(resetToFirstPage: true);

        await _notificationService.SendAsync(
            LocalizationService.GetString("MemberCreatedSuccessTitle"),
            string.Format(
                LocalizationService.Culture,
                LocalizationService.GetString("MemberCreatedSuccessMessage"),
                member.FullName),
            NotificationType.Success);
    }

    private async Task HandleMemberUpdatedAsync(MemberModel member)
    {
        ApplyMembershipState(member);
        ApplyFiltersAndSorting(resetToFirstPage: false);

        await _notificationService.SendAsync(
            LocalizationService.GetString("MemberUpdatedSuccessTitle"),
            string.Format(
                LocalizationService.Culture,
                LocalizationService.GetString("MemberUpdatedSuccessMessage"),
                member.FullName),
            NotificationType.Success);
    }

    private void ApplyFiltersAndSorting(bool resetToFirstPage)
    {
        IReadOnlyList<MemberModel> filteredMembers = _memberFilterService.Apply(_allMembers, BuildCurrentFilterCriteria());

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filteredMembers = filteredMembers
                .Where(MatchesSearch)
                .ToList();
        }

        _filteredMembers = SortMembers(filteredMembers);
        _pagination.TotalItems = _filteredMembers.Count;

        if (resetToFirstPage)
        {
            if (_pagination.CurrentPage != 1)
            {
                _pagination.CurrentPage = 1;
                return;
            }

            RefreshPagedMembers();
            return;
        }

        RefreshPagedMembers();
    }

    private MemberFilter BuildCurrentFilterCriteria()
    {
        return new MemberFilter
        {
            MembershipRank = _activeMembershipRankFilter,
            TotalSpentMin = _activeTotalSpentMinFilter,
            TotalSpentMax = _activeTotalSpentMaxFilter,
        };
    }

    private bool MatchesSearch(MemberModel member)
    {
        string membershipRankName = member.MembershipRank?.Name ?? string.Empty;

        return LocalizationService.Culture.CompareInfo.IndexOf(member.FullName, SearchText, CompareOptions.IgnoreCase) >= 0
            || LocalizationService.Culture.CompareInfo.IndexOf(member.PhoneNumber, SearchText, CompareOptions.IgnoreCase) >= 0
            || LocalizationService.Culture.CompareInfo.IndexOf(member.Code, SearchText, CompareOptions.IgnoreCase) >= 0
            || LocalizationService.Culture.CompareInfo.IndexOf(membershipRankName, SearchText, CompareOptions.IgnoreCase) >= 0;
    }

    private IReadOnlyList<MemberModel> SortMembers(IEnumerable<MemberModel> members)
    {
        bool isDescending = string.Equals(SelectedSortDirection, SortDescending, StringComparison.Ordinal);

        IOrderedEnumerable<MemberModel> orderedMembers = (SelectedSortField ?? SortFieldName) switch
        {
            SortFieldSpent => isDescending
                ? members.OrderByDescending(member => member.TotalSpentAmount).ThenBy(member => member.FullName, StringComparer.CurrentCultureIgnoreCase)
                : members.OrderBy(member => member.TotalSpentAmount).ThenBy(member => member.FullName, StringComparer.CurrentCultureIgnoreCase),
            SortFieldPackage => isDescending
                ? members.OrderByDescending(member => member.MembershipRank?.Priority ?? 0).ThenBy(member => member.FullName, StringComparer.CurrentCultureIgnoreCase)
                : members.OrderBy(member => member.MembershipRank?.Priority ?? 0).ThenBy(member => member.FullName, StringComparer.CurrentCultureIgnoreCase),
            SortFieldProgress => isDescending
                ? members.OrderByDescending(member => member.ProgressPercentage).ThenBy(member => member.FullName, StringComparer.CurrentCultureIgnoreCase)
                : members.OrderBy(member => member.ProgressPercentage).ThenBy(member => member.FullName, StringComparer.CurrentCultureIgnoreCase),
            _ => isDescending
                ? members.OrderByDescending(member => member.FullName, StringComparer.CurrentCultureIgnoreCase)
                : members.OrderBy(member => member.FullName, StringComparer.CurrentCultureIgnoreCase),
        };

        return orderedMembers.ToList();
    }

    private void RefreshPagedMembers()
    {
        PagedMemberCards.Clear();

        int startIndex = Math.Max(0, (_pagination.CurrentPage - 1) * _pagination.PageSize);
        foreach (MemberModel member in _filteredMembers.Skip(startIndex).Take(_pagination.PageSize))
        {
            if (_cardViewModelsByMember.TryGetValue(member, out MemberCardControlViewModel? viewModel))
            {
                PagedMemberCards.Add(viewModel);
            }
        }

        OnPropertyChanged(nameof(HasMembers));
        UpdatePageMetadata();
    }

    private void RefreshSortOptions()
    {
        _isUpdatingSelectionOptions = true;

        try
        {
            string currentSortField = SelectedSortField ?? SortFieldName;
            string currentSortDirection = SelectedSortDirection ?? SortAscending;

            ReplaceOptions(
                SortFieldOptions,
                [
                    new LocalizationOptionModel { Value = SortFieldName, DisplayName = LocalizationService.GetString("MemberManagementPageSortFieldNameOption") },
                    new LocalizationOptionModel { Value = SortFieldSpent, DisplayName = LocalizationService.GetString("MemberManagementPageSortFieldSpentOption") },
                    new LocalizationOptionModel { Value = SortFieldPackage, DisplayName = LocalizationService.GetString("MemberManagementPageSortFieldPackageOption") },
                    new LocalizationOptionModel { Value = SortFieldProgress, DisplayName = LocalizationService.GetString("MemberManagementPageSortFieldProgressOption") },
                ]);

            ReplaceOptions(
                SortDirectionOptions,
                [
                    new LocalizationOptionModel { Value = SortAscending, DisplayName = LocalizationService.GetString("MemberManagementPageSortDirectionAscendingOption") },
                    new LocalizationOptionModel { Value = SortDescending, DisplayName = LocalizationService.GetString("MemberManagementPageSortDirectionDescendingOption") },
                ]);

            SelectedSortField = SortFieldOptions.Any(option => option.Value == currentSortField)
                ? currentSortField
                : SortFieldName;
            SelectedSortDirection = SortDirectionOptions.Any(option => option.Value == currentSortDirection)
                ? currentSortDirection
                : SortAscending;
        }
        finally
        {
            _isUpdatingSelectionOptions = false;
        }
    }

    private void UpdatePageMetadata()
    {
        int totalItems = _filteredMembers.Count;
        int startItem = totalItems == 0 ? 0 : ((_pagination.CurrentPage - 1) * _pagination.PageSize) + 1;
        int endItem = totalItems == 0 ? 0 : Math.Min(_pagination.CurrentPage * _pagination.PageSize, totalItems);
        int totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)Math.Max(_pagination.PageSize, 1)));

        PageInfoText = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("MemberManagementPagePageInfoFormat"),
            startItem,
            endItem,
            totalItems,
            _pagination.CurrentPage,
            totalPages);
    }

    private MemberCardControlViewModel CreateCardViewModel(MemberModel member)
    {
        return _memberCardControlViewModelFactory.Create(
            member,
            HandleEditMemberAsync,
            HandleDeleteMemberAsync);
    }

    private void AddCardViewModelForMember(MemberModel member)
    {
        _cardViewModelsByMember[member] = CreateCardViewModel(member);
    }

    private void RemoveCardViewModelForMember(MemberModel member)
    {
        if (_cardViewModelsByMember.Remove(member, out MemberCardControlViewModel? viewModel))
        {
            viewModel.Dispose();
        }
    }

    private MemberModel CreateNewMemberDraft()
    {
        var member = new MemberModel
        {
            Code = GenerateNextMemberCode(),
            FullName = string.Empty,
            PhoneNumber = string.Empty,
            TotalSpentAmount = 0m,
        };

        ApplyMembershipState(member);
        return member;
    }

    private string GenerateNextMemberCode()
    {
        int currentMaxValue = _allMembers
            .Select(member =>
            {
                string digits = new string(member.Code.Where(char.IsDigit).ToArray());
                return int.TryParse(digits, out int parsedValue) ? parsedValue : 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        return $"#{currentMaxValue + 1:0000}";
    }

    private void NormalizeMembershipRanks()
    {
        _allMembershipRanks.Sort((left, right) =>
        {
            int minSpentComparison = left.MinSpentAmount.CompareTo(right.MinSpentAmount);
            return minSpentComparison != 0
                ? minSpentComparison
                : string.Compare(left.Name, right.Name, StringComparison.CurrentCultureIgnoreCase);
        });

        for (int index = 0; index < _allMembershipRanks.Count; index++)
        {
            MembershipRank rank = _allMembershipRanks[index];
            rank.Priority = index + 1;
            rank.IsDefault = index == 0;
            if (string.IsNullOrWhiteSpace(rank.Color))
            {
                rank.Color = MembershipPackageDialogViewModel.ResolveRankColor(rank.Name, index);
            }
        }

        if (_activeMembershipRankFilter is not null
            && !_allMembershipRanks.Any(rank => string.Equals(rank.Name, _activeMembershipRankFilter.Name, StringComparison.OrdinalIgnoreCase)))
        {
            _activeMembershipRankFilter = null;
        }
    }

    private void RefreshMembershipStates()
    {
        foreach (MemberModel member in _allMembers)
        {
            ApplyMembershipState(member);
        }
    }

    private void ApplyMembershipState(MemberModel member)
    {
        MemberRankProgressSnapshot snapshot = MemberRankProgressCalculator.Calculate(member.TotalSpentAmount, _allMembershipRanks);

        member.MembershipRank = null;
        member.MembershipRank = snapshot.CurrentRank;

        member.NextMembershipRank = null;
        member.NextMembershipRank = snapshot.NextRank;

        member.ProgressPercentage = snapshot.ProgressPercentage;
    }

    private static void ReplaceOptions(
        ObservableCollection<LocalizationOptionModel> collection,
        IReadOnlyList<LocalizationOptionModel> items)
    {
        collection.Clear();
        foreach (LocalizationOptionModel item in items)
        {
            collection.Add(item);
        }
    }
}
