using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Entities;
using Domain.Enums;
using WinUI.Helpers;
using WinUI.UIModels;
using WinUI.UIModels.AreaManagement;
using WinUI.UIModels.Enums;

namespace WinUI.ViewModels.Dialogs.Management;

public partial class ReservationViewModel : UpsertDialogViewModelBase
{
    private readonly IRepository<Member> _memberRepository;
    private readonly ILocalizationPreferencesService _localizationPreferencesService;
    private readonly IDialogService _dialogService;
    private AreaModel _targetModel = new();
    private Func<AreaModel, Task>? _onSubmittedAsync;
    private event Action? CloseRequestedInternal;

    public event Action? DialogHideRequested;
    public event Action? DialogShowRequested;

    public ReservationViewModel(
        ILocalizationService localizationService,
        ILocalizationPreferencesService localizationPreferencesService,
        IRepository<Member> memberRepository,
        IDialogService dialogService,
        UpsertDialogMode mode)
        : base(localizationService, mode)
    {
        _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
        _localizationPreferencesService = localizationPreferencesService ?? throw new ArgumentNullException(nameof(localizationPreferencesService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        Members = new ObservableCollection<Member>();
        LoadMembers();
        ApplyModel(_targetModel);
    }

    [ObservableProperty]
    public partial string MemberToggleText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MemberLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MemberPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    [NotifyPropertyChangedFor(nameof(ShowMemberSelection))]
    [NotifyPropertyChangedFor(nameof(ShowCustomerFields))]
    public partial bool IsMember { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial Member? SelectedMember { get; set; }

    [ObservableProperty]
    public partial string CustomerNameLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CustomerNamePlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial string CustomerName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PhoneNumberLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PhoneNumberPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial string PhoneNumber { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReservationDateLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReservationDatePlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    [NotifyPropertyChangedFor(nameof(HasReservationDate))]
    [NotifyPropertyChangedFor(nameof(ReservationDateDisplayText))]
    public partial DateTimeOffset? ReservationDate { get; set; }

    [ObservableProperty]
    public partial string ReservationTimeLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReservationTimePlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    [NotifyPropertyChangedFor(nameof(HasReservationTime))]
    [NotifyPropertyChangedFor(nameof(ReservationTimeDisplayText))]
    public partial TimeSpan? ReservationTime { get; set; }

    [ObservableProperty]
    public partial string ReservationCapacityLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReservationCapacityPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial string ReservationCapacity { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReservationPriceLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ReservationPriceText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipText { get; set; } = string.Empty;

    protected override string CreateTitleLocKey => "ReservationDialogTitleText";

    protected override string EditTitleLocKey => "ReservationDialogEditTitleText";

    public ObservableCollection<Member> Members { get; }

    public bool CanSubmit => TryGetParsedFormValues(out _, out _, out _, out _);

    public bool HasReservationDate => ReservationDate.HasValue;

    public bool HasReservationTime => ReservationTime.HasValue;

    public bool ShowMemberSelection => IsMember;

    public bool ShowCustomerFields => !IsMember;

    public string ReservationDateDisplayText => ReservationDate.HasValue
        ? FormatReservationDate(ReservationDate.Value)
        : ReservationDatePlaceholderText;

    public string ReservationTimeDisplayText => ReservationTime.HasValue
        ? DateTime.Today.Add(ReservationTime.Value).ToString("HH:mm", LocalizationService.Culture)
        : ReservationTimePlaceholderText;

    public event Action? CloseRequested
    {
        add => CloseRequestedInternal += value;
        remove => CloseRequestedInternal -= value;
    }

    protected override void RefreshLocalizedText()
    {
        base.RefreshLocalizedText();

        MemberToggleText = LocalizationService.GetString("StartSessionDialogMemberToggleText");
        MemberLabelText = LocalizationService.GetString("StartSessionDialogMemberLabelText");
        MemberPlaceholderText = LocalizationService.GetString("StartSessionDialogMemberPlaceholderText");
        CustomerNameLabelText = LocalizationService.GetString("ReservationDialogNameLabelText");
        CustomerNamePlaceholderText = LocalizationService.GetString("ReservationDialogNamePlaceholderText");
        PhoneNumberLabelText = LocalizationService.GetString("ReservationDialogPhoneLabelText");
        PhoneNumberPlaceholderText = LocalizationService.GetString("ReservationDialogPhonePlaceholderText");
        ReservationDateLabelText = LocalizationService.GetString("ReservationDialogDateLabelText");
        ReservationDatePlaceholderText = LocalizationService.GetString("ReservationDialogDatePlaceholderText");
        ReservationTimeLabelText = LocalizationService.GetString("ReservationDialogTimeLabelText");
        ReservationTimePlaceholderText = LocalizationService.GetString("ReservationDialogTimePlaceholderText");
        ReservationCapacityLabelText = LocalizationService.GetString("ReservationCustomerNumberLabelText");
        ReservationCapacityPlaceholderText = ReservationCapacityLabelText.Trim().TrimEnd(':');
        ReservationPriceLabelText = LocalizationService.GetString("ReservationDialogReservationPriceLabelText");
        CloseTooltipText = LocalizationService.GetString("CloseTooltipText");
        SubmitButtonText = IsEdit
            ? LocalizationService.GetString("SaveButtonText")
            : LocalizationService.GetString("ReservationDialogConfirmButtonText");

        UpdateReservationPriceText();
        OnPropertyChanged(nameof(ReservationDateDisplayText));
        OnPropertyChanged(nameof(ReservationTimeDisplayText));
    }

    public void Configure(ReservationDialogRequest? request)
    {
        _onSubmittedAsync = request?.OnSubmittedAsync;
        _targetModel = request?.Model ?? new AreaModel();

        Icon = new IconState
        {
            Kind = _targetModel.PlayAreaType == PlayAreaType.Room ? IconKind.Room : IconKind.Table,
            Size = 24,
            AlwaysFilled = true,
        };

        ApplyModel(_targetModel);
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
        }
        else
        {
            DialogShowRequested?.Invoke();
        }
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        if (!TryBuildUpdatedModel(out AreaModel updatedModel))
        {
            return;
        }

        DialogHideRequested?.Invoke();

        bool isConfirmed = await _dialogService.ShowConfirmationAsync(
            titleKey: IsEdit ? "ConfirmEditTitle" : "ConfirmAddTitle",
            messageKey: IsEdit ? "ConfirmEditMessage" : "ConfirmAddMessage",
            confirmButtonTextKey: IsEdit ? "ConfirmEditButton" : "ConfirmAddButton",
            cancelButtonTextKey: "CancelButtonText");

        if (!isConfirmed)
        {
            DialogShowRequested?.Invoke();
            return;
        }

        try
        {
            ApplyReservationToTarget(updatedModel);

            if (_onSubmittedAsync is not null)
            {
                await _onSubmittedAsync(_targetModel);
            }

            CloseRequestedInternal?.Invoke();
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync(ex.Message);
            DialogShowRequested?.Invoke();
        }
    }

    partial void OnCustomerNameChanged(string value) => NotifyFormStateChanged();

    partial void OnPhoneNumberChanged(string value) => NotifyFormStateChanged();

    partial void OnReservationDateChanged(DateTimeOffset? value) => NotifyFormStateChanged();

    partial void OnReservationTimeChanged(TimeSpan? value) => NotifyFormStateChanged();

    partial void OnReservationCapacityChanged(string value) => NotifyFormStateChanged();

    partial void OnIsMemberChanged(bool value) => NotifyFormStateChanged();

    partial void OnSelectedMemberChanged(Member? value) => NotifyFormStateChanged();

    [RelayCommand(CanExecute = nameof(CanDecrementReservationCapacity))]
    private void DecrementReservationCapacity()
    {
        int capacity = ParseReservationCapacityOrDefault();
        if (capacity <= 1)
        {
            return;
        }

        ReservationCapacity = (capacity - 1).ToString(LocalizationService.Culture);
    }

    [RelayCommand(CanExecute = nameof(CanIncrementReservationCapacity))]
    private void IncrementReservationCapacity()
    {
        int capacity = ParseReservationCapacityOrDefault();
        int upperBound = GetCapacityUpperBound();
        if (capacity >= upperBound)
        {
            return;
        }

        ReservationCapacity = Math.Min(upperBound, capacity + 1).ToString(LocalizationService.Culture);
    }

    private string FormatReservationDate(DateTimeOffset value)
    {
        string dateFormat = _localizationPreferencesService.Preferences.DateFormat;
        if (string.IsNullOrWhiteSpace(dateFormat))
        {
            dateFormat = LocalizationPreferences.DefaultDateFormat;
        }

        try
        {
            return value.ToString(dateFormat, LocalizationService.Culture);
        }
        catch (FormatException)
        {
            return value.ToString(LocalizationPreferences.DefaultDateFormat, LocalizationService.Culture);
        }
    }

    private void ApplyModel(AreaModel model)
    {
        Member? selectedMember = FindMemberById(model.MemberId);
        IsMember = selectedMember is not null;
        SelectedMember = selectedMember;
        CustomerName = model.CustomerName;
        PhoneNumber = model.PhoneNumber;

        if (model.CheckInDateTime is DateTime checkInDateTime)
        {
            var reservationDateTime = new DateTimeOffset(checkInDateTime);
            ReservationDate = ReservationDateTimeHelper.CreateDate(reservationDateTime);
            ReservationTime = reservationDateTime.TimeOfDay;
        }
        else
        {
            ReservationDate = null;
            ReservationTime = null;
        }

        ReservationCapacity = model.Capacity > 0
            ? ClampCapacity(model.Capacity).ToString(LocalizationService.Culture)
            : "1";

        UpdateReservationPriceText();
        NotifyFormStateChanged();
    }

    private void LoadMembers()
    {
        var members = _memberRepository.GetAllAsync().GetAwaiter().GetResult()
            .Where(member => member.IsActive)
            .OrderBy(member => member.FullName)
            .ToList();

        Members.Clear();
        foreach (Member member in members)
        {
            Members.Add(member);
        }
    }

    private Member? FindMemberById(string? memberId)
    {
        if (string.IsNullOrWhiteSpace(memberId))
        {
            return null;
        }

        return Members.FirstOrDefault(member => string.Equals(member.Id, memberId, StringComparison.Ordinal));
    }

    private void UpdateReservationPriceText()
    {
        decimal reservationPrice = _targetModel.ReservationPrice > 0m
            ? _targetModel.ReservationPrice
            : 60000m;

        ReservationPriceText = LocalizationService.FormatCurrency(reservationPrice);
    }

    private void NotifyFormStateChanged()
    {
        SubmitCommand.NotifyCanExecuteChanged();
        DecrementReservationCapacityCommand.NotifyCanExecuteChanged();
        IncrementReservationCapacityCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanSubmit));
        OnPropertyChanged(nameof(ShowMemberSelection));
        OnPropertyChanged(nameof(ShowCustomerFields));
    }

    private bool TryBuildUpdatedModel(out AreaModel model)
    {
        model = _targetModel.Clone();

        if (!TryGetParsedFormValues(out string trimmedCustomerName, out string trimmedPhoneNumber, out int capacity, out DateTime reservationDateTime))
        {
            return false;
        }

        model.CustomerName = trimmedCustomerName;
        model.PhoneNumber = trimmedPhoneNumber;
        model.MemberId = IsMember ? SelectedMember?.Id : null;
        model.CheckInDateTime = reservationDateTime;
        model.Capacity = capacity;
        model.StartTime = null;
        model.IsSessionPaused = false;
        model.SessionPausedAt = null;
        model.SessionPausedDuration = TimeSpan.Zero;
        model.TotalAmount = 0m;
        model.Status = PlayAreaStatus.Reserved;

        return true;
    }

    private void ApplyReservationToTarget(AreaModel updatedModel)
    {
        _targetModel.CustomerName = updatedModel.CustomerName;
        _targetModel.PhoneNumber = updatedModel.PhoneNumber;
        _targetModel.MemberId = updatedModel.MemberId;
        _targetModel.CheckInDateTime = updatedModel.CheckInDateTime;
        _targetModel.Capacity = updatedModel.Capacity;
        _targetModel.StartTime = updatedModel.StartTime;
        _targetModel.IsSessionPaused = updatedModel.IsSessionPaused;
        _targetModel.SessionPausedAt = updatedModel.SessionPausedAt;
        _targetModel.SessionPausedDuration = updatedModel.SessionPausedDuration;
        _targetModel.TotalAmount = updatedModel.TotalAmount;
        _targetModel.Status = updatedModel.Status;
    }

    private bool TryGetParsedFormValues(
        out string trimmedCustomerName,
        out string trimmedPhoneNumber,
        out int capacity,
        out DateTime reservationDateTime)
    {
        trimmedCustomerName = IsMember
            ? SelectedMember?.FullName?.Trim() ?? string.Empty
            : CustomerName?.Trim() ?? string.Empty;
        trimmedPhoneNumber = IsMember
            ? SelectedMember?.PhoneNumber?.Trim() ?? string.Empty
            : PhoneNumber?.Trim() ?? string.Empty;
        reservationDateTime = default;

        if ((IsMember && SelectedMember is null)
            || (!IsMember && string.IsNullOrWhiteSpace(trimmedCustomerName))
            || (!IsMember && string.IsNullOrWhiteSpace(trimmedPhoneNumber))
            || !ReservationDate.HasValue
            || !ReservationTime.HasValue
            || !TryParseCapacity(ReservationCapacity, out capacity)
            || capacity <= 0
            || capacity > GetCapacityUpperBound()
            || !ReservationDateTimeHelper.IsValidSelection(ReservationDate, ReservationTime))
        {
            capacity = 0;
            return false;
        }

        reservationDateTime = DateTime.SpecifyKind(
            ReservationDate.Value.LocalDateTime.Date + ReservationTime.Value,
            DateTimeKind.Local);

        return true;
    }

    private bool TryParseCapacity(string? text, out int capacity)
    {
        const NumberStyles styles = NumberStyles.Integer;

        return int.TryParse(text, styles, LocalizationService.Culture, out capacity)
            || int.TryParse(text, styles, CultureInfo.InvariantCulture, out capacity);
    }

    private bool CanDecrementReservationCapacity() => ParseReservationCapacityOrDefault() > 1;

    private bool CanIncrementReservationCapacity() => ParseReservationCapacityOrDefault() < GetCapacityUpperBound();

    private int ParseReservationCapacityOrDefault()
    {
        return TryParseCapacity(ReservationCapacity, out int capacity) && capacity > 0
            ? ClampCapacity(capacity)
            : 1;
    }

    private int ClampCapacity(int capacity)
    {
        return Math.Clamp(capacity, 1, GetCapacityUpperBound());
    }

    private int GetCapacityUpperBound()
    {
        return _targetModel.MaxCapacity > 0
            ? Math.Max(1, _targetModel.MaxCapacity)
            : int.MaxValue;
    }
}
