using System;
using System.Globalization;
using System.Threading.Tasks;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Enums;
using WinUI.UIModels.AreaManagement;
using WinUI.UIModels.Enums;

namespace WinUI.ViewModels.Dialogs.Management;

public partial class AreaDialogViewModel : UpsertDialogViewModelBase
{
    private AreaModel _targetModel = new();
    private AreaModel _initialModel = new();
    private Func<AreaModel, Task>? _onSubmittedAsync;
    private event Action? CloseRequestedInternal;

    // Events to handle hiding/showing the dialog from the View Model.
    public event Action? DialogHideRequested;
    public event Action? DialogShowRequested;

    private readonly IDialogService _dialogService;

    public AreaDialogViewModel(
        ILocalizationService localizationService,
        IDialogService dialogService,
        UpsertDialogMode mode)
        : base(localizationService, mode)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        ApplyModel(_initialModel);
    }

    [ObservableProperty]
    public partial string NameLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NamePlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TypeLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TableTypeText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string RoomTypeText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MaxCapacityLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string HourlyPriceLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string HourlyPricePlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ResetButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial string AreaName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTableTypeSelected))]
    [NotifyPropertyChangedFor(nameof(IsRoomTypeSelected))]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial PlayAreaType SelectedPlayAreaType { get; set; } = PlayAreaType.Table;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial string MaxCapacityText { get; set; } = "2";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial string HourlyPriceText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string ErrorMessage { get; set; } = string.Empty;

    protected override string CreateTitleLocKey => "AreaDialogCreateTitleText";

    protected override string EditTitleLocKey => "AreaDialogEditTitleText";

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool CanSubmit => TryGetParsedFormValues(out _, out _, out _);

    public bool IsTableTypeSelected
    {
        get => SelectedPlayAreaType == PlayAreaType.Table;
        set
        {
            if (value)
            {
                SelectedPlayAreaType = PlayAreaType.Table;
            }
            OnPropertyChanged(nameof(IsTableTypeSelected));
        }
    }

    public bool IsRoomTypeSelected
    {
        get => SelectedPlayAreaType == PlayAreaType.Room;
        set
        {
            if (value)
            {
                SelectedPlayAreaType = PlayAreaType.Room;
            }
            OnPropertyChanged(nameof(IsRoomTypeSelected));
        }
    }

    public event Action? CloseRequested
    {
        add => CloseRequestedInternal += value;
        remove => CloseRequestedInternal -= value;
    }

    protected override void RefreshLocalizedText()
    {
        base.RefreshLocalizedText();

        NameLabelText = LocalizationService.GetString("AreaDialogNameLabelText");
        NamePlaceholderText = LocalizationService.GetString("AreaDialogNamePlaceholderText");
        TypeLabelText = LocalizationService.GetString("AreaDialogTypeLabelText");
        TableTypeText = LocalizationService.GetString("AreaDialogTableTypeText");
        RoomTypeText = LocalizationService.GetString("AreaDialogRoomTypeText");
        MaxCapacityLabelText = LocalizationService.GetString("AreaDialogMaxCapacityLabelText");
        HourlyPriceLabelText = LocalizationService.GetString("AreaDialogHourlyPriceLabelText");
        HourlyPricePlaceholderText = LocalizationService.GetString("AreaDialogHourlyPricePlaceholderText");
        ResetButtonText = LocalizationService.GetString("AreaDialogResetButtonText");
        CloseTooltipText = LocalizationService.GetString("CloseTooltipText");
        SubmitButtonText = IsEdit
            ? LocalizationService.GetString("SaveButtonText")
            : LocalizationService.GetString("AreaDialogApplyButtonText");
    }

    public void Configure(AreaDialogRequest? request)
    {
        _onSubmittedAsync = request?.OnSubmittedAsync;
        _targetModel = request?.Model ?? new AreaModel();
        _initialModel = _targetModel.Clone();
        ErrorMessage = string.Empty;
        ApplyModel(_initialModel);
    }

    public override Task SaveAsync() => SubmitAsync();

    [RelayCommand]
    private async Task CloseAsync()
    {
        DialogHideRequested?.Invoke();

        bool isConfirmed = await _dialogService.ShowConfirmationAsync(
            titleKey: "ConfirmCloseTitle",
            messageKey: "ConfirmCloseMessage",
            confirmButtonTextKey: "ConfirmCloseButton",
            cancelButtonTextKey: "CancelButtonText"
        );

        if (isConfirmed)
        {
            CloseRequestedInternal?.Invoke();
        }
        else
        {
            DialogShowRequested?.Invoke();
        }
    }

    [RelayCommand]
    private void Reset()
    {
        ErrorMessage = string.Empty;
        ApplyModel(_initialModel);
    }

    [RelayCommand(CanExecute = nameof(CanDecrementCapacity))]
    private void DecrementCapacity()
    {
        if (!TryParseMaxCapacity(MaxCapacityText, out int maxCapacity) || maxCapacity <= 1)
        {
            return;
        }

        MaxCapacityText = (maxCapacity - 1).ToString(LocalizationService.Culture);
    }

    [RelayCommand]
    private void IncrementCapacity()
    {
        int maxCapacity = TryParseMaxCapacity(MaxCapacityText, out int parsedCapacity)
            ? Math.Max(1, parsedCapacity)
            : 1;

        MaxCapacityText = (maxCapacity + 1).ToString(LocalizationService.Culture);
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        if (!TryApplyToTargetModel(out AreaModel model))
        {
            return;
        }

        string confirmTitleKey = IsEdit ? "ConfirmEditTitle" : "ConfirmAddTitle";
        string confirmMessageKey = IsEdit ? "ConfirmEditMessage" : "ConfirmAddMessage";

        DialogHideRequested?.Invoke();

        bool isConfirmed = await _dialogService.ShowConfirmationAsync(
            titleKey: confirmTitleKey,
            messageKey: confirmMessageKey,
            confirmButtonTextKey: IsEdit ? "ConfirmEditButton" : "ConfirmAddButton",
            cancelButtonTextKey: "CancelButtonText"
        );

        if (!isConfirmed)
        {
            DialogShowRequested?.Invoke();
            return;
        }

        ErrorMessage = string.Empty;

        try
        {
            if (_onSubmittedAsync is not null)
            {
                await _onSubmittedAsync(model);
            }

            CloseRequestedInternal?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private bool CanDecrementCapacity() => TryParseMaxCapacity(MaxCapacityText, out int maxCapacity) && maxCapacity > 1;

    partial void OnAreaNameChanged(string value) => NotifyFormStateChanged();

    partial void OnSelectedPlayAreaTypeChanged(PlayAreaType value) => NotifyFormStateChanged();

    partial void OnMaxCapacityTextChanged(string value)
    {
        DecrementCapacityCommand.NotifyCanExecuteChanged();
        NotifyFormStateChanged();
    }

    partial void OnHourlyPriceTextChanged(string value) => NotifyFormStateChanged();

    private void ApplyModel(AreaModel model)
    {
        AreaName = model.AreaName;
        SelectedPlayAreaType = model.PlayAreaType;
        MaxCapacityText = Math.Max(1, model.MaxCapacity).ToString(LocalizationService.Culture);
        HourlyPriceText = model.HourlyPrice > 0m
            ? model.HourlyPrice.ToString("0.##", LocalizationService.Culture)
            : string.Empty;
        NotifyFormStateChanged();
        DecrementCapacityCommand.NotifyCanExecuteChanged();
    }

    private void NotifyFormStateChanged()
    {
        if (!string.IsNullOrWhiteSpace(ErrorMessage))
        {
            ErrorMessage = string.Empty;
        }

        SubmitCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanSubmit));
        OnPropertyChanged(nameof(IsTableTypeSelected));
        OnPropertyChanged(nameof(IsRoomTypeSelected));
    }

    private bool TryApplyToTargetModel(out AreaModel model)
    {
        model = _targetModel;

        if (!TryGetParsedFormValues(out string trimmedName, out int maxCapacity, out decimal hourlyPrice))
        {
            return false;
        }

        model.AreaName = trimmedName;
        model.PlayAreaType = SelectedPlayAreaType;
        model.MaxCapacity = maxCapacity;
        model.HourlyPrice = hourlyPrice;

        return true;
    }

    private bool TryGetParsedFormValues(out string trimmedName, out int maxCapacity, out decimal hourlyPrice)
    {
        trimmedName = AreaName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            maxCapacity = 0;
            hourlyPrice = 0m;
            return false;
        }

        if (!TryParseMaxCapacity(MaxCapacityText, out maxCapacity) || maxCapacity <= 0)
        {
            maxCapacity = 0;
            hourlyPrice = 0m;
            return false;
        }

        if (!TryParseHourlyPrice(HourlyPriceText, out hourlyPrice) || hourlyPrice <= 0m)
        {
            maxCapacity = 0;
            hourlyPrice = 0m;
            return false;
        }

        return true;
    }

    private bool TryParseMaxCapacity(string? text, out int maxCapacity)
    {
        const NumberStyles styles = NumberStyles.Integer;

        return int.TryParse(text, styles, LocalizationService.Culture, out maxCapacity)
            || int.TryParse(text, styles, CultureInfo.InvariantCulture, out maxCapacity);
    }

    private bool TryParseHourlyPrice(string? text, out decimal hourlyPrice)
    {
        const NumberStyles styles = NumberStyles.Number;

        return decimal.TryParse(text, styles, LocalizationService.Culture, out hourlyPrice)
            || decimal.TryParse(text, styles, CultureInfo.InvariantCulture, out hourlyPrice);
    }
}
