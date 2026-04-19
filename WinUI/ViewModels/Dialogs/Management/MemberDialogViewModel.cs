using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Application.Members;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Entities;
using WinUI.Helpers.Validations;
using WinUI.Services.Factories;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;

namespace WinUI.ViewModels.Dialogs.Management;

public partial class MemberDialogViewModel : UpsertDialogViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly MemberModelFactory _memberModelFactory;
    private IReadOnlyList<MembershipRank> _availableMembershipRanks = [];
    private MemberModel _targetModel = new();
    private MemberModel _initialModel = new();
    private Func<MemberModel, Task>? _onSubmittedAsync;
    private event Action? CloseRequestedInternal;

    public event Action? DialogHideRequested;

    public event Action? DialogShowRequested;

    public MemberDialogViewModel(
        ILocalizationService localizationService,
        IDialogService dialogService,
        MemberModelFactory memberModelFactory,
        UpsertDialogMode mode)
        : base(localizationService, mode)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _memberModelFactory = memberModelFactory ?? throw new ArgumentNullException(nameof(memberModelFactory));
        ApplyModel(_initialModel);
    }

    [ObservableProperty]
    public partial string NameLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NamePlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PhoneNumberLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PhoneNumberPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TotalSpentLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string TotalSpentPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MembershipPreviewLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProgressPreviewLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ResetButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial string MemberName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial string PhoneNumberText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    [NotifyPropertyChangedFor(nameof(CurrentMembershipPreviewText))]
    [NotifyPropertyChangedFor(nameof(ProgressPreviewText))]
    public partial string TotalSpentText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string ErrorMessage { get; set; } = string.Empty;

    protected override string CreateTitleLocKey => "MemberDialogCreateTitleText";

    protected override string EditTitleLocKey => "MemberDialogEditTitleText";

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool CanSubmit => TryGetParsedFormValues(out _, out _, out _);

    public string CurrentMembershipPreviewText => BuildPreview().CurrentRank?.Name
        ?? LocalizationService.GetString("MemberDialogNoMembershipPreviewText");

    public string ProgressPreviewText => string.Format(
        LocalizationService.Culture,
        LocalizationService.GetString("MemberDialogProgressPreviewFormat"),
        BuildPreview().ProgressPercentage);

    public event Action? CloseRequested
    {
        add => CloseRequestedInternal += value;
        remove => CloseRequestedInternal -= value;
    }

    protected override void RefreshLocalizedText()
    {
        base.RefreshLocalizedText();

        NameLabelText = LocalizationService.GetString("MemberDialogNameLabelText");
        NamePlaceholderText = LocalizationService.GetString("MemberDialogNamePlaceholderText");
        PhoneNumberLabelText = LocalizationService.GetString("MemberDialogPhoneNumberLabelText");
        PhoneNumberPlaceholderText = LocalizationService.GetString("MemberDialogPhoneNumberPlaceholderText");
        TotalSpentLabelText = LocalizationService.GetString("MemberDialogTotalSpentLabelText");
        TotalSpentPlaceholderText = LocalizationService.GetString("MemberDialogTotalSpentPlaceholderText");
        MembershipPreviewLabelText = LocalizationService.GetString("MemberDialogMembershipPreviewLabelText");
        ProgressPreviewLabelText = LocalizationService.GetString("MemberDialogProgressPreviewLabelText");
        ResetButtonText = LocalizationService.GetString("MemberDialogResetButtonText");
        CloseTooltipText = LocalizationService.GetString("CloseTooltipText");
        SubmitButtonText = IsEdit
            ? LocalizationService.GetString("SaveButtonText")
            : LocalizationService.GetString("MemberDialogApplyButtonText");
    }

    public void Configure(MemberDialogRequest? request)
    {
        _availableMembershipRanks = request?.AvailableMembershipRanks?.ToList() ?? [];
        _onSubmittedAsync = request?.OnSubmittedAsync;
        _targetModel = request?.Model ?? CreateDefaultModel();
        _initialModel = _memberModelFactory.Clone(_targetModel);
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
            cancelButtonTextKey: "CancelButtonText");

        if (isConfirmed)
        {
            CloseRequestedInternal?.Invoke();
            return;
        }

        DialogShowRequested?.Invoke();
    }

    [RelayCommand]
    private void Reset()
    {
        ErrorMessage = string.Empty;
        ApplyModel(_initialModel);
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        if (!TryApplyToTargetModel(out MemberModel model))
        {
            ErrorMessage = LocalizationService.GetString("MemberDialogInvalidInputText");
            return;
        }

        DialogHideRequested?.Invoke();

        bool isConfirmed = await _dialogService.ShowConfirmationAsync(
            titleKey: IsEdit ? "ConfirmEditMemberTitle" : "ConfirmAddMemberTitle",
            messageKey: IsEdit ? "ConfirmEditMemberMessage" : "ConfirmAddMemberMessage",
            confirmButtonTextKey: IsEdit ? "ConfirmEditMemberButton" : "ConfirmAddMemberButton",
            cancelButtonTextKey: "CancelButtonText");

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
            DialogShowRequested?.Invoke();
        }
    }

    partial void OnMemberNameChanged(string value) => NotifyFormStateChanged();

    partial void OnPhoneNumberTextChanged(string value) => NotifyFormStateChanged();

    partial void OnTotalSpentTextChanged(string value) => NotifyFormStateChanged();

    private void ApplyModel(MemberModel model)
    {
        MemberName = model.FullName;
        PhoneNumberText = model.PhoneNumber;
        TotalSpentText = model.TotalSpentAmount > 0m
            ? model.TotalSpentAmount.ToString("0.##", LocalizationService.Culture)
            : string.Empty;
        NotifyFormStateChanged();
    }

    private void NotifyFormStateChanged()
    {
        if (!string.IsNullOrWhiteSpace(ErrorMessage))
        {
            ErrorMessage = string.Empty;
        }

        SubmitCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanSubmit));
        OnPropertyChanged(nameof(CurrentMembershipPreviewText));
        OnPropertyChanged(nameof(ProgressPreviewText));
    }

    private MemberModel CreateDefaultModel()
    {
        return new MemberModel
        {
            Code = string.Empty,
            MembershipRank = _availableMembershipRanks.OrderBy(rank => rank.MinSpentAmount).FirstOrDefault(),
            NextMembershipRank = _availableMembershipRanks.OrderBy(rank => rank.MinSpentAmount).Skip(1).FirstOrDefault(),
        };
    }

    private bool TryApplyToTargetModel(out MemberModel model)
    {
        model = _targetModel;

        if (!TryGetParsedFormValues(out string trimmedName, out string trimmedPhoneNumber, out decimal totalSpentAmount))
        {
            return false;
        }

        MemberRankProgressSnapshot preview = BuildPreview(totalSpentAmount);

        model.FullName = trimmedName;
        model.PhoneNumber = trimmedPhoneNumber;
        model.TotalSpentAmount = totalSpentAmount;
        model.MembershipRank = preview.CurrentRank;
        model.NextMembershipRank = preview.NextRank;
        model.ProgressPercentage = preview.ProgressPercentage;

        return true;
    }

    private bool TryGetParsedFormValues(out string trimmedName, out string trimmedPhoneNumber, out decimal totalSpentAmount)
    {
        trimmedName = MemberName?.Trim() ?? string.Empty;
        trimmedPhoneNumber = PhoneNumberText?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmedName)
            || !PhoneValidation.IsValid(trimmedPhoneNumber)
            || !TryParseRequiredDecimal(TotalSpentText, out totalSpentAmount))
        {
            totalSpentAmount = 0m;
            return false;
        }

        return totalSpentAmount >= 0m;
    }

    private MemberRankProgressSnapshot BuildPreview()
    {
        return TryParseRequiredDecimal(TotalSpentText, out decimal totalSpentAmount)
            ? BuildPreview(totalSpentAmount)
            : BuildPreview(0m);
    }

    private MemberRankProgressSnapshot BuildPreview(decimal totalSpentAmount)
    {
        return MemberRankProgressCalculator.Calculate(totalSpentAmount, _availableMembershipRanks);
    }

    private bool TryParseRequiredDecimal(string? text, out decimal value)
    {
        string trimmedText = text?.Trim() ?? string.Empty;

        return (decimal.TryParse(trimmedText, System.Globalization.NumberStyles.Number, LocalizationService.Culture, out value)
                || decimal.TryParse(trimmedText, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out value))
               && value >= 0m;
    }
}
