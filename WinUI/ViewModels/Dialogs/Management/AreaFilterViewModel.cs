using System;
using System.Globalization;
using System.Threading.Tasks;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Enums;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels;

namespace WinUI.ViewModels.Dialogs.Management;

public sealed class AreaFilterCriteria
{
    public PlayAreaStatus? Status { get; init; }

    public TimeSpan? StartTimeFrom { get; init; }

    public TimeSpan? StartTimeTo { get; init; }

    public int? CapacityMin { get; init; }

    public int? CapacityMax { get; init; }

    public decimal? HourlyPriceMin { get; init; }

    public decimal? HourlyPriceMax { get; init; }
}

public sealed class AreaFilterDialogRequest
{
    public AreaFilterCriteria? InitialCriteria { get; init; }

    public Func<AreaFilterCriteria, Task>? OnSubmittedAsync { get; init; }
}

public partial class AreaFilterViewModel : LocalizedViewModelBase
{
    private Func<AreaFilterCriteria, Task>? _onSubmittedAsync;
    private event Action? CloseRequestedInternal;

    public AreaFilterViewModel(ILocalizationService localizationService)
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
    public partial string StatusLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StartTimeRangeLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CapacityRangeLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string HourlyPriceRangeLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StartTimeFromPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StartTimeToPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CapacityFromPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CapacityToPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string HourlyPriceFromPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string HourlyPriceToPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AllStatusesText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AvailableStatusText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReservedStatusText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RentedStatusText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ResetButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ApplyButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAllStatusesSelected))]
    [NotifyPropertyChangedFor(nameof(IsAvailableStatusSelected))]
    [NotifyPropertyChangedFor(nameof(IsReservedStatusSelected))]
    [NotifyPropertyChangedFor(nameof(IsRentedStatusSelected))]
    public partial PlayAreaStatus? SelectedStatus { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStartTimeFrom))]
    [NotifyPropertyChangedFor(nameof(StartTimeFromDisplayText))]
    public partial TimeSpan? StartTimeFrom { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasStartTimeTo))]
    [NotifyPropertyChangedFor(nameof(StartTimeToDisplayText))]
    public partial TimeSpan? StartTimeTo { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApply))]
    public partial string CapacityMinText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApply))]
    public partial string CapacityMaxText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApply))]
    public partial string HourlyPriceMinText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApply))]
    public partial string HourlyPriceMaxText { get; set; } = string.Empty;

    public bool IsAllStatusesSelected
    {
        get => SelectedStatus is null;
        set
        {
            if (value)
            {
                SelectedStatus = null;
            }
        }
    }

    public bool IsAvailableStatusSelected
    {
        get => SelectedStatus == PlayAreaStatus.Available;
        set
        {
            if (value)
            {
                SelectedStatus = PlayAreaStatus.Available;
            }
        }
    }

    public bool IsReservedStatusSelected
    {
        get => SelectedStatus == PlayAreaStatus.Reserved;
        set
        {
            if (value)
            {
                SelectedStatus = PlayAreaStatus.Reserved;
            }
        }
    }

    public bool IsRentedStatusSelected
    {
        get => SelectedStatus == PlayAreaStatus.Rented;
        set
        {
            if (value)
            {
                SelectedStatus = PlayAreaStatus.Rented;
            }
        }
    }

    public bool HasStartTimeFrom => StartTimeFrom.HasValue;

    public bool HasStartTimeTo => StartTimeTo.HasValue;

    public string StartTimeFromDisplayText => StartTimeFrom.HasValue
        ? FormatTime(StartTimeFrom.Value)
        : StartTimeFromPlaceholderText;

    public string StartTimeToDisplayText => StartTimeTo.HasValue
        ? FormatTime(StartTimeTo.Value)
        : StartTimeToPlaceholderText;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool CanApply => HasValidNumericRanges();

    public event Action? CloseRequested
    {
        add => CloseRequestedInternal += value;
        remove => CloseRequestedInternal -= value;
    }

    public void Configure(AreaFilterDialogRequest? request)
    {
        _onSubmittedAsync = request?.OnSubmittedAsync;

        AreaFilterCriteria criteria = request?.InitialCriteria ?? new AreaFilterCriteria();
        SelectedStatus = criteria.Status;
        StartTimeFrom = criteria.StartTimeFrom;
        StartTimeTo = criteria.StartTimeTo;
        CapacityMinText = criteria.CapacityMin?.ToString(LocalizationService.Culture) ?? string.Empty;
        CapacityMaxText = criteria.CapacityMax?.ToString(LocalizationService.Culture) ?? string.Empty;
        HourlyPriceMinText = criteria.HourlyPriceMin?.ToString("0.##", LocalizationService.Culture) ?? string.Empty;
        HourlyPriceMaxText = criteria.HourlyPriceMax?.ToString("0.##", LocalizationService.Culture) ?? string.Empty;
        ErrorMessage = string.Empty;
    }

    protected override void RefreshLocalizedText()
    {
        TitleText = LocalizationService.GetString("AreaFilterDialogTitleText");
        StatusLabelText = LocalizationService.GetString("AreaFilterDialogStatusLabelText");
        StartTimeRangeLabelText = LocalizationService.GetString("AreaFilterDialogStartTimeRangeLabelText");
        CapacityRangeLabelText = LocalizationService.GetString("AreaFilterDialogCapacityRangeLabelText");
        HourlyPriceRangeLabelText = LocalizationService.GetString("AreaFilterDialogHourlyPriceRangeLabelText");
        StartTimeFromPlaceholderText = LocalizationService.GetString("AreaFilterDialogStartTimeFromPlaceholderText");
        StartTimeToPlaceholderText = LocalizationService.GetString("AreaFilterDialogStartTimeToPlaceholderText");
        CapacityFromPlaceholderText = LocalizationService.GetString("AreaFilterDialogCapacityFromPlaceholderText");
        CapacityToPlaceholderText = LocalizationService.GetString("AreaFilterDialogCapacityToPlaceholderText");
        HourlyPriceFromPlaceholderText = LocalizationService.GetString("AreaFilterDialogHourlyPriceFromPlaceholderText");
        HourlyPriceToPlaceholderText = LocalizationService.GetString("AreaFilterDialogHourlyPriceToPlaceholderText");
        AllStatusesText = LocalizationService.GetString("AreaFilterDialogAllStatusesText");
        AvailableStatusText = LocalizationService.GetString("AreaManagementAvailableStatusText");
        ReservedStatusText = LocalizationService.GetString("AreaManagementReservedStatusText");
        RentedStatusText = LocalizationService.GetString("AreaManagementRentedStatusText");
        ResetButtonText = LocalizationService.GetString("AreaFilterDialogResetButtonText");
        ApplyButtonText = LocalizationService.GetString("AreaFilterDialogApplyButtonText");
        CloseTooltipText = LocalizationService.GetString("CloseTooltipText");

        OnPropertyChanged(nameof(StartTimeFromDisplayText));
        OnPropertyChanged(nameof(StartTimeToDisplayText));
    }

    [RelayCommand(CanExecute = nameof(CanApply))]
    private async Task ApplyAsync()
    {
        if (!TryBuildCriteria(out AreaFilterCriteria criteria))
        {
            ErrorMessage = LocalizationService.GetString("AreaFilterDialogInvalidRangeText");
            OnPropertyChanged(nameof(HasError));
            return;
        }

        ErrorMessage = string.Empty;
        OnPropertyChanged(nameof(HasError));

        if (_onSubmittedAsync is not null)
        {
            await _onSubmittedAsync(criteria);
        }

        CloseRequestedInternal?.Invoke();
    }

    [RelayCommand]
    private void Reset()
    {
        SelectedStatus = null;
        StartTimeFrom = null;
        StartTimeTo = null;
        CapacityMinText = string.Empty;
        CapacityMaxText = string.Empty;
        HourlyPriceMinText = string.Empty;
        HourlyPriceMaxText = string.Empty;
        ErrorMessage = string.Empty;
        NotifyFilterInputChanged();
    }

    [RelayCommand]
    private void Close()
    {
        CloseRequestedInternal?.Invoke();
    }

    partial void OnSelectedStatusChanged(PlayAreaStatus? value) => NotifyFilterInputChanged();

    partial void OnStartTimeFromChanged(TimeSpan? value)
    {
        OnPropertyChanged(nameof(HasStartTimeFrom));
        OnPropertyChanged(nameof(StartTimeFromDisplayText));
        NotifyFilterInputChanged();
    }

    partial void OnStartTimeToChanged(TimeSpan? value)
    {
        OnPropertyChanged(nameof(HasStartTimeTo));
        OnPropertyChanged(nameof(StartTimeToDisplayText));
        NotifyFilterInputChanged();
    }

    partial void OnCapacityMinTextChanged(string value) => NotifyFilterInputChanged();

    partial void OnCapacityMaxTextChanged(string value) => NotifyFilterInputChanged();

    partial void OnHourlyPriceMinTextChanged(string value) => NotifyFilterInputChanged();

    partial void OnHourlyPriceMaxTextChanged(string value) => NotifyFilterInputChanged();

    private void NotifyFilterInputChanged()
    {
        if (!string.IsNullOrWhiteSpace(ErrorMessage))
        {
            ErrorMessage = string.Empty;
            OnPropertyChanged(nameof(HasError));
        }

        ApplyCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanApply));
    }

    private bool HasValidNumericRanges()
    {
        return TryParseOptionalInt(CapacityMinText, out _)
            && TryParseOptionalInt(CapacityMaxText, out _)
            && TryParseOptionalDecimal(HourlyPriceMinText, out _)
            && TryParseOptionalDecimal(HourlyPriceMaxText, out _);
    }

    private bool TryBuildCriteria(out AreaFilterCriteria criteria)
    {
        criteria = new AreaFilterCriteria();

        if (!TryParseOptionalInt(CapacityMinText, out int? capacityMin)
            || !TryParseOptionalInt(CapacityMaxText, out int? capacityMax)
            || !TryParseOptionalDecimal(HourlyPriceMinText, out decimal? hourlyPriceMin)
            || !TryParseOptionalDecimal(HourlyPriceMaxText, out decimal? hourlyPriceMax))
        {
            return false;
        }

        (TimeSpan? startTimeFrom, TimeSpan? startTimeTo) = NormalizeRange(StartTimeFrom, StartTimeTo);
        (int? normalizedCapacityMin, int? normalizedCapacityMax) = NormalizeRange(capacityMin, capacityMax);
        (decimal? normalizedHourlyPriceMin, decimal? normalizedHourlyPriceMax) = NormalizeRange(hourlyPriceMin, hourlyPriceMax);

        criteria = new AreaFilterCriteria
        {
            Status = SelectedStatus,
            StartTimeFrom = startTimeFrom,
            StartTimeTo = startTimeTo,
            CapacityMin = normalizedCapacityMin,
            CapacityMax = normalizedCapacityMax,
            HourlyPriceMin = normalizedHourlyPriceMin,
            HourlyPriceMax = normalizedHourlyPriceMax,
        };

        return true;
    }

    private string FormatTime(TimeSpan value)
    {
        return DateTime.Today.Add(value).ToString("HH:mm", LocalizationService.Culture);
    }

    private bool TryParseOptionalInt(string? text, out int? value)
    {
        const NumberStyles styles = NumberStyles.Integer;
        string trimmedText = text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmedText))
        {
            value = null;
            return true;
        }

        bool success =
            int.TryParse(trimmedText, styles, LocalizationService.Culture, out int parsedValue)
            || int.TryParse(trimmedText, styles, CultureInfo.InvariantCulture, out parsedValue);

        value = success ? Math.Max(0, parsedValue) : null;
        return success;
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
}
