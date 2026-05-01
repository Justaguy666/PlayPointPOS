using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Application.Members;
using Application.Services;
using Application.Services.Members;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinUI.Services.Factories;
using WinUI.Services.Management;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;
using WinUI.ViewModels.Common;
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

    private readonly MemberManagementDialogCoordinator _dialogs;
    private readonly IMemberFilterService _memberFilterService;
    private readonly IMembershipRankManagementService _rankManagementService;
    private readonly MemberDraftFactory _draftFactory;
    private readonly MemberCardControlViewModelFactory _cardFactory;
    private readonly List<MemberModel> _allMembers;
    private readonly List<MembershipRank> _allMembershipRanks;
    private readonly ManagementQueryState<MemberFilter, ManagementSortState> _queryState;
    private readonly ManagementCollectionController<MemberModel, MemberCardControlViewModel> _members;
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

    // PERF: Bắt buộc dùng ObservableCollection cho ListView/ItemsRepeater để UI tự động render lại (Re-render) 
    // các item bị thay đổi thay vì phải xóa toàn bộ control và vẽ lại từ đầu, giúp tăng FPS khi scroll hoặc phân trang.
    public ObservableCollection<MemberCardControlViewModel> PagedMemberCards { get; } = [];

    public ObservableCollection<LocalizationOptionModel> SortFieldOptions { get; } = [];

    public ObservableCollection<LocalizationOptionModel> SortDirectionOptions { get; } = [];

    public PaginationControlViewModel PaginationViewModel { get; }

    public PaginationModel Pagination { get; }

    public IconKind SearchIconKind => IconKind.Search;

    public bool HasMembers => _members.HasItems;

    public IAsyncRelayCommand FilterCommand { get; }

    public IAsyncRelayCommand AddMemberCommand { get; }

    public IAsyncRelayCommand ManageMembershipPackagesCommand { get; }

    public IRelayCommand ClearSearchCommand { get; }

    public MemberManagementPageViewModel(
        ILocalizationService localizationService,
        MemberManagementDialogCoordinator dialogs,
        IMemberCatalogService memberCatalogService,
        IMembershipRankCatalogService membershipRankCatalogService,
        IMemberFilterService memberFilterService,
        IMembershipRankManagementService rankManagementService,
        MemberModelFactory memberModelFactory,
        MemberDraftFactory draftFactory,
        MemberCardControlViewModelFactory cardFactory,
        PaginationControlViewModel paginationViewModel)
        : base(localizationService)
    {
        _dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));
        _memberFilterService = memberFilterService ?? throw new ArgumentNullException(nameof(memberFilterService));
        _rankManagementService = rankManagementService ?? throw new ArgumentNullException(nameof(rankManagementService));
        _draftFactory = draftFactory ?? throw new ArgumentNullException(nameof(draftFactory));
        _cardFactory = cardFactory ?? throw new ArgumentNullException(nameof(cardFactory));
        PaginationViewModel = paginationViewModel ?? throw new ArgumentNullException(nameof(paginationViewModel));
        ArgumentNullException.ThrowIfNull(memberCatalogService);
        ArgumentNullException.ThrowIfNull(membershipRankCatalogService);
        ArgumentNullException.ThrowIfNull(memberModelFactory);

        Pagination = new PaginationModel { CurrentPage = 1, PageSize = PageSize, MaxVisiblePageButtons = 4 };
        PaginationViewModel.Pagination = Pagination;
        _queryState = new ManagementQueryState<MemberFilter, ManagementSortState>(
            new MemberFilter(),
            new ManagementSortState(SortFieldName, SortAscending));

        _allMembershipRanks = membershipRankCatalogService.GetMembershipRanks().ToList();
        _rankManagementService.NormalizeRanks(_allMembershipRanks);
        _allMembers = memberCatalogService.GetMembers()
            .Select(memberModelFactory.Create)
            .ToList();
        _rankManagementService.RefreshMembershipStates(_allMembers, _allMembershipRanks);

        _members = new ManagementCollectionController<MemberModel, MemberCardControlViewModel>(
            _allMembers,
            Pagination,
            PagedMemberCards,
            QueryMembers,
            CreateCardViewModel);
        _members.Refreshed += HandleMembersRefreshed;

        FilterCommand = new AsyncRelayCommand(OpenFilterDialogAsync);
        AddMemberCommand = new AsyncRelayCommand(ExecuteAddMemberAsync);
        ManageMembershipPackagesCommand = new AsyncRelayCommand(ExecuteManageMembershipPackagesAsync);
        ClearSearchCommand = new RelayCommand(ClearSearch);

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

        _members.Refreshed -= HandleMembersRefreshed;
        _members.Dispose();
        PaginationViewModel.Dispose();
        _isDisposed = true;
        base.Dispose();
    }

    partial void OnSearchTextChanged(string value)
    {
        if (_isInitialized)
        {
            _queryState.SearchText = value;
            ApplyFiltersAndSorting(resetToFirstPage: true);
        }
    }

    partial void OnSelectedSortFieldChanged(string? value) => HandleSortChanged();

    partial void OnSelectedSortDirectionChanged(string? value) => HandleSortChanged();

    private void HandleSortChanged()
    {
        if (!_isInitialized || _isUpdatingSelectionOptions)
        {
            return;
        }

        SyncSortState();
        ApplyFiltersAndSorting(resetToFirstPage: true);
    }

    private Task OpenFilterDialogAsync()
    {
        return _dialogs.OpenFilterAsync(_allMembershipRanks, _queryState.Filter, HandleFilterSubmittedAsync);
    }

    private Task ExecuteAddMemberAsync()
    {
        return _dialogs.OpenAddAsync(_draftFactory.Create(_allMembers, _allMembershipRanks), _allMembershipRanks, HandleMemberCreatedAsync);
    }

    private Task ExecuteManageMembershipPackagesAsync()
    {
        return _dialogs.OpenMembershipPackagesAsync(
            new ObservableCollection<MembershipRank>(_allMembershipRanks),
            HandleMembershipRankAddedAsync,
            HandleMembershipRankDeletedAsync,
            HandleMembershipRankUpdatedAsync);
    }

    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    private Task HandleEditMemberAsync(MemberModel member)
    {
        return member is null
            ? Task.CompletedTask
            : _dialogs.OpenEditAsync(member, _allMembershipRanks, HandleMemberUpdatedAsync);
    }

    private async Task HandleDeleteMemberAsync(MemberModel member)
    {
        if (member is null || !await _dialogs.ConfirmDeleteAsync())
        {
            return;
        }

        if (!_members.Remove(member))
        {
            return;
        }

        ApplyFiltersAndSorting(resetToFirstPage: false);
        await _dialogs.NotifyDeletedAsync(member);
    }

    private Task HandleFilterSubmittedAsync(MemberFilter criteria)
    {
        _queryState.Filter = criteria;
        ApplyFiltersAndSorting(resetToFirstPage: true);
        return Task.CompletedTask;
    }

    private async Task HandleMemberCreatedAsync(MemberModel member)
    {
        _rankManagementService.ApplyMembershipState(member, _allMembershipRanks);
        if (!_members.Contains(member))
        {
            _members.Insert(0, member);
        }

        ApplyFiltersAndSorting(resetToFirstPage: true);
        await _dialogs.NotifyCreatedAsync(member);
    }

    private async Task HandleMemberUpdatedAsync(MemberModel member)
    {
        _rankManagementService.ApplyMembershipState(member, _allMembershipRanks);
        ApplyFiltersAndSorting(resetToFirstPage: false);
        await _dialogs.NotifyUpdatedAsync(member);
    }

    private Task HandleMembershipRankAddedAsync(MembershipRank rank)
    {
        if (!_allMembershipRanks.Contains(rank))
        {
            _allMembershipRanks.Add(rank);
        }

        RefreshMembershipRankState();
        return Task.CompletedTask;
    }

    private Task HandleMembershipRankDeletedAsync(MembershipRank rank)
    {
        _rankManagementService.DeleteRank(_allMembershipRanks, rank);
        RefreshMembershipRankState();
        return Task.CompletedTask;
    }

    private Task HandleMembershipRankUpdatedAsync(MembershipRank rank)
    {
        RefreshMembershipRankState();
        return Task.CompletedTask;
    }

    private void RefreshMembershipRankState()
    {
        _rankManagementService.NormalizeRanks(_allMembershipRanks);
        if (_queryState.Filter.MembershipRank is not null
            && !_allMembershipRanks.Any(rank => IsSameMembershipRank(rank, _queryState.Filter.MembershipRank)))
        {
            _queryState.Filter = _queryState.Filter with { MembershipRank = null };
        }

        _rankManagementService.RefreshMembershipStates(_allMembers, _allMembershipRanks);
        ApplyFiltersAndSorting(resetToFirstPage: false);
    }

    private void ApplyFiltersAndSorting(bool resetToFirstPage)
    {
        _queryState.SearchText = SearchText;
        SyncSortState();
        _members.Refresh(resetToFirstPage);
    }

    private IReadOnlyList<MemberModel> QueryMembers(IEnumerable<MemberModel> source)
    {
        IEnumerable<MemberModel> members = _memberFilterService.Apply(source, _queryState.Filter);

        if (!string.IsNullOrWhiteSpace(_queryState.SearchText))
        {
            members = members.Where(MatchesSearch);
        }

        return SortMembers(members);
    }

    private bool MatchesSearch(MemberModel member)
    {
        string membershipRankName = member.MembershipRank?.Name ?? string.Empty;
        return ManagementCollectionFlow.ContainsSearchText(LocalizationService.Culture, member.FullName, _queryState.SearchText)
            || ManagementCollectionFlow.ContainsSearchText(LocalizationService.Culture, member.PhoneNumber, _queryState.SearchText)
            || ManagementCollectionFlow.ContainsSearchText(LocalizationService.Culture, member.Code, _queryState.SearchText)
            || ManagementCollectionFlow.ContainsSearchText(LocalizationService.Culture, membershipRankName, _queryState.SearchText);
    }

    private IReadOnlyList<MemberModel> SortMembers(IEnumerable<MemberModel> members)
    {
        bool isDescending = string.Equals(_queryState.Sort.Direction, SortDescending, StringComparison.Ordinal);
        IOrderedEnumerable<MemberModel> orderedMembers = (_queryState.Sort.Field ?? SortFieldName) switch
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

    private void RefreshSortOptions()
    {
        _isUpdatingSelectionOptions = true;

        try
        {
            string currentSortField = SelectedSortField ?? SortFieldName;
            string currentSortDirection = SelectedSortDirection ?? SortAscending;
            ManagementCollectionFlow.ReplaceOptions(
                SortFieldOptions,
                [
                    new LocalizationOptionModel { Value = SortFieldName, DisplayName = LocalizationService.GetString("MemberManagementPageSortFieldNameOption") },
                    new LocalizationOptionModel { Value = SortFieldSpent, DisplayName = LocalizationService.GetString("MemberManagementPageSortFieldSpentOption") },
                    new LocalizationOptionModel { Value = SortFieldPackage, DisplayName = LocalizationService.GetString("MemberManagementPageSortFieldPackageOption") },
                    new LocalizationOptionModel { Value = SortFieldProgress, DisplayName = LocalizationService.GetString("MemberManagementPageSortFieldProgressOption") },
                ]);
            ManagementCollectionFlow.ReplaceOptions(
                SortDirectionOptions,
                [
                    new LocalizationOptionModel { Value = SortAscending, DisplayName = LocalizationService.GetString("MemberManagementPageSortDirectionAscendingOption") },
                    new LocalizationOptionModel { Value = SortDescending, DisplayName = LocalizationService.GetString("MemberManagementPageSortDirectionDescendingOption") },
                ]);

            SelectedSortField = SortFieldOptions.Any(option => option.Value == currentSortField) ? currentSortField : SortFieldName;
            SelectedSortDirection = SortDirectionOptions.Any(option => option.Value == currentSortDirection) ? currentSortDirection : SortAscending;
            SyncSortState();
        }
        finally
        {
            _isUpdatingSelectionOptions = false;
        }
    }

    private void UpdatePageMetadata()
    {
        PageInfoSnapshot pageInfo = _members.BuildPageInfo();
        PageInfoText = string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("MemberManagementPagePageInfoFormat"),
            pageInfo.StartItem,
            pageInfo.EndItem,
            pageInfo.TotalItems,
            pageInfo.CurrentPage,
            pageInfo.TotalPages);
    }

    private MemberCardControlViewModel CreateCardViewModel(MemberModel member)
    {
        return _cardFactory.Create(member, HandleEditMemberAsync, HandleDeleteMemberAsync);
    }

    private void HandleMembersRefreshed()
    {
        OnPropertyChanged(nameof(HasMembers));
        UpdatePageMetadata();
    }

    private void SyncSortState()
    {
        _queryState.Sort = new ManagementSortState(SelectedSortField, SelectedSortDirection);
    }

    private static bool IsSameMembershipRank(MembershipRank? left, MembershipRank? right)
    {
        return left is not null
            && right is not null
            && string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
    }
}
