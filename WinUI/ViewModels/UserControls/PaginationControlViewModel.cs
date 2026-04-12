using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Application.Services;
using Application.UseCases.Pagination;
using Application.UseCases.Pagination.Contracts;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using WinUI.Resources;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels;

namespace WinUI.ViewModels.UserControls;

public sealed class PaginationControlViewModel : LocalizedViewModelBase
{
    private readonly BuildPaginationStateUseCase _buildPaginationStateUseCase;
    private readonly Brush _selectedPageBackgroundBrush;
    private readonly Brush _selectedPageBorderBrush;
    private readonly Brush _selectedPageForegroundBrush;
    private readonly Brush _unselectedPageBackgroundBrush;
    private readonly Brush _unselectedPageBorderBrush;
    private readonly Brush _unselectedPageForegroundBrush;

    private PaginationModel _pagination = new();
    private ObservableCollection<PaginationPageButtonModel> _pageButtons = [];
    private IconState _jumpToPageIconState = new()
    {
        Kind = IconKind.Up,
        Size = 24,
        AlwaysFilled = true,
    };
    private string _jumpToPageButtonText = string.Empty;
    private string _jumpToPageTitleText = string.Empty;
    private string _cancelButtonText = string.Empty;
    private string _confirmButtonText = string.Empty;
    private string _jumpPageInputText = string.Empty;
    private string _jumpPageRangeText = string.Empty;
    private int _totalPages = 1;
    private bool _isJumpToPageOpen;
    private bool _isApplyingPaginationState;
    private bool _isDisposed;

    public PaginationControlViewModel(
        ILocalizationService localizationService,
        BuildPaginationStateUseCase buildPaginationStateUseCase)
        : base(localizationService)
    {
        _buildPaginationStateUseCase = buildPaginationStateUseCase ?? throw new ArgumentNullException(nameof(buildPaginationStateUseCase));

        _selectedPageBackgroundBrush = AppResourceLookup.GetBrush("OrangeFocusBrush", AppColors.OrangeFocus);
        _selectedPageBorderBrush = AppResourceLookup.GetBrush("OrangeFocusBrush", AppColors.OrangeFocus);
        _selectedPageForegroundBrush = AppResourceLookup.GetBrush("WhiteBrush", AppColors.White);
        _unselectedPageBackgroundBrush = AppResourceLookup.GetBrush("WhiteBrush", AppColors.White);
        _unselectedPageBorderBrush = AppResourceLookup.GetBrush("LightGrayBrush", AppColors.LightGray);
        _unselectedPageForegroundBrush = AppResourceLookup.GetBrush("PrimaryOrangeBrush", AppColors.PrimaryOrange);

        GoToFirstPageCommand = new RelayCommand(() => ChangePage(1), CanGoToFirstPage);
        GoToPreviousPageCommand = new RelayCommand(() => ChangePage(Pagination.CurrentPage - 1), CanGoToPreviousPage);
        SelectPageCommand = new RelayCommand<int>(ChangePage);
        GoToNextPageCommand = new RelayCommand(() => ChangePage(Pagination.CurrentPage + 1), CanGoToNextPage);
        GoToLastPageCommand = new RelayCommand(() => ChangePage(TotalPages), CanGoToLastPage);
        OpenJumpToPageCommand = new RelayCommand(OpenJumpToPage);
        CloseJumpToPageCommand = new RelayCommand(CloseJumpToPage);
        ConfirmJumpToPageCommand = new RelayCommand(ConfirmJumpToPage, CanConfirmJumpToPage);

        _pagination.PropertyChanged += HandlePaginationPropertyChanged;

        RefreshLocalizedText();
        RebuildPaginationState();
    }

    public PaginationModel Pagination
    {
        get => _pagination;
        set
        {
            var nextModel = value ?? new PaginationModel();
            if (ReferenceEquals(_pagination, nextModel))
                return;

            _pagination.PropertyChanged -= HandlePaginationPropertyChanged;
            _pagination = nextModel;
            _pagination.PropertyChanged += HandlePaginationPropertyChanged;

            OnPropertyChanged();
            RebuildPaginationState();
        }
    }

    public ObservableCollection<PaginationPageButtonModel> PageButtons
    {
        get => _pageButtons;
        private set => SetProperty(ref _pageButtons, value);
    }

    public IconState JumpToPageIconState
    {
        get => _jumpToPageIconState;
        private set => SetProperty(ref _jumpToPageIconState, value);
    }

    public string JumpToPageButtonText
    {
        get => _jumpToPageButtonText;
        private set => SetProperty(ref _jumpToPageButtonText, value);
    }

    public string JumpToPageTitleText
    {
        get => _jumpToPageTitleText;
        private set => SetProperty(ref _jumpToPageTitleText, value);
    }

    public string CancelButtonText
    {
        get => _cancelButtonText;
        private set => SetProperty(ref _cancelButtonText, value);
    }

    public string ConfirmButtonText
    {
        get => _confirmButtonText;
        private set => SetProperty(ref _confirmButtonText, value);
    }

    public string JumpPageInputText
    {
        get => _jumpPageInputText;
        set
        {
            if (!SetProperty(ref _jumpPageInputText, value))
                return;

            ConfirmJumpToPageCommand.NotifyCanExecuteChanged();
        }
    }

    public string JumpPageRangeText
    {
        get => _jumpPageRangeText;
        private set => SetProperty(ref _jumpPageRangeText, value);
    }

    public int TotalPages
    {
        get => _totalPages;
        private set => SetProperty(ref _totalPages, value);
    }

    public bool IsJumpToPageOpen
    {
        get => _isJumpToPageOpen;
        set
        {
            if (!SetProperty(ref _isJumpToPageOpen, value))
                return;

            RefreshJumpToPageIconState();
        }
    }

    public IRelayCommand GoToFirstPageCommand { get; }

    public IRelayCommand GoToPreviousPageCommand { get; }

    public IRelayCommand<int> SelectPageCommand { get; }

    public IRelayCommand GoToNextPageCommand { get; }

    public IRelayCommand GoToLastPageCommand { get; }

    public IRelayCommand OpenJumpToPageCommand { get; }

    public IRelayCommand CloseJumpToPageCommand { get; }

    public IRelayCommand ConfirmJumpToPageCommand { get; }

    protected override void RefreshLocalizedText()
    {
        JumpToPageButtonText = GetLocalizedText("PaginationGoToPageButtonText");
        JumpToPageTitleText = GetLocalizedText("PaginationJumpToPageTitleText");
        CancelButtonText = GetLocalizedText("PaginationCancelButtonText");
        ConfirmButtonText = GetLocalizedText("PaginationConfirmButtonText");
    }

    public new void Dispose()
    {
        if (_isDisposed)
            return;

        _pagination.PropertyChanged -= HandlePaginationPropertyChanged;
        _isDisposed = true;
        base.Dispose();
    }

    private void HandlePaginationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isApplyingPaginationState)
            return;

        if (e.PropertyName is nameof(PaginationModel.CurrentPage) or
            nameof(PaginationModel.TotalItems) or
            nameof(PaginationModel.PageSize) or
            nameof(PaginationModel.MaxVisiblePageButtons))
        {
            RebuildPaginationState();
        }
    }

    private void RebuildPaginationState()
    {
        BuildPaginationStateResult state = _buildPaginationStateUseCase.Execute(
            new BuildPaginationStateRequest(
                Pagination.CurrentPage,
                Pagination.TotalItems,
                Pagination.PageSize,
                Pagination.MaxVisiblePageButtons));

        ApplyPaginationState(state);
    }

    private void ApplyPaginationState(BuildPaginationStateResult state)
    {
        _isApplyingPaginationState = true;

        try
        {
            if (Pagination.TotalItems != state.TotalItems)
                Pagination.TotalItems = state.TotalItems;

            if (Pagination.PageSize != state.PageSize)
                Pagination.PageSize = state.PageSize;

            if (Pagination.MaxVisiblePageButtons != state.MaxVisiblePages)
                Pagination.MaxVisiblePageButtons = state.MaxVisiblePages;

            if (Pagination.CurrentPage != state.CurrentPage)
                Pagination.CurrentPage = state.CurrentPage;
        }
        finally
        {
            _isApplyingPaginationState = false;
        }

        TotalPages = state.TotalPages;
        JumpPageRangeText = string.Format(
            LocalizationService.Culture,
            "{0} - {1}",
            1,
            state.TotalPages);

        PageButtons = new ObservableCollection<PaginationPageButtonModel>(
            state.VisiblePages.Select(CreatePageButtonModel));

        NotifyCommandStates();
    }

    private PaginationPageButtonModel CreatePageButtonModel(int pageNumber)
    {
        bool isSelected = pageNumber == Pagination.CurrentPage;

        return new PaginationPageButtonModel
        {
            PageNumber = pageNumber,
            Label = pageNumber.ToString(LocalizationService.Culture),
            IsSelected = isSelected,
            Background = isSelected ? _selectedPageBackgroundBrush : _unselectedPageBackgroundBrush,
            BorderBrush = isSelected ? _selectedPageBorderBrush : _unselectedPageBorderBrush,
            Foreground = isSelected ? _selectedPageForegroundBrush : _unselectedPageForegroundBrush,
        };
    }

    private void ChangePage(int pageNumber)
    {
        int targetPage = Math.Clamp(pageNumber, 1, TotalPages);
        if (Pagination.CurrentPage == targetPage)
            return;

        Pagination.CurrentPage = targetPage;
    }

    private bool CanGoToFirstPage() => Pagination.CurrentPage > 1;

    private bool CanGoToPreviousPage() => Pagination.CurrentPage > 1;

    private bool CanGoToNextPage() => Pagination.CurrentPage < TotalPages;

    private bool CanGoToLastPage() => Pagination.CurrentPage < TotalPages;

    private void OpenJumpToPage()
    {
        JumpPageInputText = string.Empty;
        IsJumpToPageOpen = !IsJumpToPageOpen;
    }

    private void CloseJumpToPage()
    {
        IsJumpToPageOpen = false;
        JumpPageInputText = string.Empty;
    }

    private void ConfirmJumpToPage()
    {
        if (!TryParseJumpPage(out int requestedPage))
            return;

        ChangePage(requestedPage);
        CloseJumpToPage();
    }

    private bool CanConfirmJumpToPage()
    {
        return TryParseJumpPage(out _);
    }

    private bool TryParseJumpPage(out int requestedPage)
    {
        if (!int.TryParse(JumpPageInputText, NumberStyles.None, LocalizationService.Culture, out requestedPage))
            return false;

        return requestedPage >= 1 && requestedPage <= TotalPages;
    }

    private void NotifyCommandStates()
    {
        GoToFirstPageCommand.NotifyCanExecuteChanged();
        GoToPreviousPageCommand.NotifyCanExecuteChanged();
        GoToNextPageCommand.NotifyCanExecuteChanged();
        GoToLastPageCommand.NotifyCanExecuteChanged();
        ConfirmJumpToPageCommand.NotifyCanExecuteChanged();
    }

    private string GetLocalizedText(string key)
    {
        string value = LocalizationService.GetString(key);
        return string.IsNullOrWhiteSpace(value) || value.StartsWith("[", StringComparison.Ordinal)
            ? key
            : value;
    }

    private void RefreshJumpToPageIconState()
    {
        JumpToPageIconState = new IconState
        {
            Kind = IsJumpToPageOpen ? IconKind.Down : IconKind.Up,
            Size = 24,
            AlwaysFilled = true,
        };
    }
}
