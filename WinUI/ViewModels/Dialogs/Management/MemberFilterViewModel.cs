using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Application.Members;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Entities;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels;

namespace WinUI.ViewModels.Dialogs.Management;

public partial class MemberFilterViewModel : LocalizedViewModelBase
{
    private const string AllOptionValue = "";

    private IReadOnlyList<MembershipRank> _availableMembershipRanks = [];
    private Func<MemberFilter, Task>? _onSubmittedAsync;
    private bool _isUpdatingSelectionOptions;
    private event Action? CloseRequestedInternal;

    public MemberFilterViewModel(ILocalizationService localizationService)
        : base(localizationService)
    {
        Icon = new IconState
        {
            Kind = IconKind.Filter,
            Size = 24,
            AlwaysFilled = true,
        };

        RefreshLocalizedText();
    }

    [ObservableProperty]
    public partial IconState Icon { get; set; } = new();

    [ObservableProperty]
    public partial string TitleText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MembershipRankLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TotalSpentRangeLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TotalSpentFromPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TotalSpentToPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AllOptionsText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ResetButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ApplyButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApply))]
    public partial string? SelectedMembershipRankValue { get; set; } = AllOptionValue;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApply))]
    public partial string TotalSpentMinText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApply))]
    public partial string TotalSpentMaxText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string ErrorMessage { get; set; } = string.Empty;

    public ObservableCollection<LocalizationOptionModel> MembershipRankOptions { get; } = [];

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool CanApply => HasValidNumericRanges();

    public event Action? CloseRequested
    {
        add => CloseRequestedInternal += value;
        remove => CloseRequestedInternal -= value;
    }

    public void Configure(MemberFilterDialogRequest? request)
    {
        _availableMembershipRanks = request?.AvailableMembershipRanks?.ToList() ?? [];
        _onSubmittedAsync = request?.OnSubmittedAsync;

        MemberFilter criteria = request?.InitialCriteria ?? new MemberFilter();
        SelectedMembershipRankValue = criteria.MembershipRank?.Name ?? AllOptionValue;
        TotalSpentMinText = criteria.TotalSpentMin?.ToString("0.##", LocalizationService.Culture) ?? string.Empty;
        TotalSpentMaxText = criteria.TotalSpentMax?.ToString("0.##", LocalizationService.Culture) ?? string.Empty;
        ErrorMessage = string.Empty;

        RefreshSelectionOptions();
    }

    protected override void RefreshLocalizedText()
    {
        TitleText = LocalizationService.GetString("MemberFilterDialogTitleText");
        MembershipRankLabelText = LocalizationService.GetString("MemberFilterDialogMembershipRankLabelText");
        TotalSpentRangeLabelText = LocalizationService.GetString("MemberFilterDialogTotalSpentRangeLabelText");
        TotalSpentFromPlaceholderText = LocalizationService.GetString("MemberFilterDialogTotalSpentFromPlaceholderText");
        TotalSpentToPlaceholderText = LocalizationService.GetString("MemberFilterDialogTotalSpentToPlaceholderText");
        AllOptionsText = LocalizationService.GetString("MemberFilterDialogAllOptionsText");
        ResetButtonText = LocalizationService.GetString("MemberFilterDialogResetButtonText");
        ApplyButtonText = LocalizationService.GetString("MemberFilterDialogApplyButtonText");
        CloseTooltipText = LocalizationService.GetString("CloseTooltipText");

        RefreshSelectionOptions();
    }

    [RelayCommand(CanExecute = nameof(CanApply))]
    private async Task ApplyAsync()
    {
        if (!TryBuildCriteria(out MemberFilter criteria))
        {
            ErrorMessage = LocalizationService.GetString("MemberFilterDialogInvalidRangeText");
            return;
        }

        ErrorMessage = string.Empty;

        if (_onSubmittedAsync is not null)
        {
            await _onSubmittedAsync(criteria);
        }

        CloseRequestedInternal?.Invoke();
    }

    [RelayCommand]
    private void Reset()
    {
        SelectedMembershipRankValue = AllOptionValue;
        TotalSpentMinText = string.Empty;
        TotalSpentMaxText = string.Empty;
        ErrorMessage = string.Empty;
        NotifyFilterInputChanged();
    }

    [RelayCommand]
    private void Close()
    {
        CloseRequestedInternal?.Invoke();
    }

    partial void OnSelectedMembershipRankValueChanged(string? value)
    {
        if (!_isUpdatingSelectionOptions)
        {
            NotifyFilterInputChanged();
        }
    }

    partial void OnTotalSpentMinTextChanged(string value) => NotifyFilterInputChanged();

    partial void OnTotalSpentMaxTextChanged(string value) => NotifyFilterInputChanged();

    private void RefreshSelectionOptions()
    {
        _isUpdatingSelectionOptions = true;

        try
        {
            string currentMembershipRankValue = SelectedMembershipRankValue ?? AllOptionValue;

            ReplaceOptions(
                MembershipRankOptions,
                new List<LocalizationOptionModel> { new() { Value = AllOptionValue, DisplayName = AllOptionsText } }
                    .Concat(_availableMembershipRanks
                        .OrderBy(rank => rank.MinSpentAmount)
                        .Select(rank => new LocalizationOptionModel
                        {
                            Value = rank.Name,
                            DisplayName = rank.Name,
                        }))
                    .ToList());

            if (!MembershipRankOptions.Any(option => option.Value == currentMembershipRankValue))
            {
                currentMembershipRankValue = AllOptionValue;
            }

            SelectedMembershipRankValue = currentMembershipRankValue;
        }
        finally
        {
            _isUpdatingSelectionOptions = false;
        }
    }

    private void NotifyFilterInputChanged()
    {
        if (!string.IsNullOrWhiteSpace(ErrorMessage))
        {
            ErrorMessage = string.Empty;
        }

        ApplyCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanApply));
    }

    private bool HasValidNumericRanges()
    {
        return TryParseOptionalDecimal(TotalSpentMinText, out _) && TryParseOptionalDecimal(TotalSpentMaxText, out _);
    }

    private bool TryBuildCriteria(out MemberFilter criteria)
    {
        criteria = new MemberFilter();

        if (!TryParseOptionalDecimal(TotalSpentMinText, out decimal? totalSpentMin)
            || !TryParseOptionalDecimal(TotalSpentMaxText, out decimal? totalSpentMax))
        {
            return false;
        }

        (decimal? normalizedMin, decimal? normalizedMax) = NormalizeRange(totalSpentMin, totalSpentMax);

        criteria = new MemberFilter
        {
            MembershipRank = ResolveSelectedMembershipRank(),
            TotalSpentMin = normalizedMin,
            TotalSpentMax = normalizedMax,
        };

        return true;
    }

    private MembershipRank? ResolveSelectedMembershipRank()
    {
        return _availableMembershipRanks.FirstOrDefault(rank =>
            string.Equals(rank.Name, SelectedMembershipRankValue, StringComparison.OrdinalIgnoreCase));
    }

    private bool TryParseOptionalDecimal(string? text, out decimal? value)
    {
        const NumberStyles styles = NumberStyles.Number;
        string trimmedText = text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmedText))
        {
            value = null;
            return true;
        }

        bool success =
            decimal.TryParse(trimmedText, styles, LocalizationService.Culture, out decimal parsedValue)
            || decimal.TryParse(trimmedText, styles, CultureInfo.InvariantCulture, out parsedValue);

        value = success ? Math.Max(0m, parsedValue) : null;
        return success;
    }

    private static (T? Min, T? Max) NormalizeRange<T>(T? min, T? max)
        where T : struct, IComparable<T>
    {
        if (min.HasValue && max.HasValue && min.Value.CompareTo(max.Value) > 0)
        {
            return (max, min);
        }

        return (min, max);
    }

    private static void ReplaceOptions(
        ObservableCollection<LocalizationOptionModel> collection,
        IReadOnlyList<LocalizationOptionModel> options)
    {
        collection.Clear();
        foreach (LocalizationOptionModel option in options)
        {
            collection.Add(option);
        }
    }
}
