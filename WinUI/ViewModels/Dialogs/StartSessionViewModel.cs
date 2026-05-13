using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Services;
using Application.Areas;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Entities;
using Domain.Enums;
using Microsoft.UI.Dispatching;
using WinUI.Helpers;
using WinUI.UIModels.Management;
using WinUI.ViewModels;

namespace WinUI.ViewModels.Dialogs;

public partial class StartSessionViewModel : LocalizedViewModelBase
{
    private readonly IRepository<Member> _memberRepository;
    private readonly IManagementApiService _managementApiService;
    private readonly INotificationService _notificationService;
    private AreaModel? _model;
    private event Action? CloseRequestedInternal;

    public StartSessionViewModel(
        ILocalizationService localizationService,
        IRepository<Member> memberRepository,
        IManagementApiService managementApiService,
        INotificationService notificationService)
        : base(localizationService)
    {
        _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
        _managementApiService = managementApiService ?? throw new ArgumentNullException(nameof(managementApiService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        Members = new ObservableCollection<Member>();
        LoadMembers();
        RefreshLocalizedText();
    }

    [ObservableProperty]
    public partial string TitleText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MemberToggleText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MemberLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MemberPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowMemberSelection))]
    [NotifyPropertyChangedFor(nameof(CanConfirm))]
    public partial bool IsMember { get; set; }

    /// <summary>Member id for ComboBox <c>SelectedValue</c> (avoids WinUI TwoWay <c>SelectedItem</c> binding issues).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConfirm))]
    public partial string? SelectedMemberId { get; set; }

    [ObservableProperty]
    public partial string CapacityLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConfirm))]
    public partial string CapacityText { get; set; } = "1";

    [ObservableProperty]
    public partial string ConfirmButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CancelButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ValidationMessageText { get; set; } = string.Empty;

    public ObservableCollection<Member> Members { get; }

    public bool ShowMemberSelection => IsMember;

    public bool HasValidationMessage => !string.IsNullOrWhiteSpace(ValidationMessageText);

    public bool CanConfirm => Validate(showMessage: false);

    public event Action? CloseRequested
    {
        add => CloseRequestedInternal += value;
        remove => CloseRequestedInternal -= value;
    }

    protected override void RefreshLocalizedText()
    {
        TitleText = LocalizationService.GetString("StartSessionDialogTitleText");
        MemberToggleText = LocalizationService.GetString("StartSessionDialogMemberToggleText");
        MemberLabelText = LocalizationService.GetString("StartSessionDialogMemberLabelText");
        MemberPlaceholderText = LocalizationService.GetString("StartSessionDialogMemberPlaceholderText");
        CapacityLabelText = LocalizationService.GetString("StartSessionDialogCapacityLabelText");
        ConfirmButtonText = LocalizationService.GetString("StartSessionDialogConfirmButtonText");
        CancelButtonText = LocalizationService.GetString("StartSessionDialogCancelButtonText");
        CloseTooltipText = LocalizationService.GetString("CloseTooltipText");
        UpdateValidationState();
    }

    public void Configure(AreaModel? model)
    {
        _model = model;
        IsMember = false;
        SelectedMemberId = null;

        int initialCapacity = model?.Capacity > 0
            ? model.Capacity
            : 1;
        CapacityText = ClampCapacity(initialCapacity).ToString(LocalizationService.Culture);
        UpdateValidationState();
    }

    [RelayCommand(CanExecute = nameof(CanIncrementCapacity))]
    private void IncrementCapacity()
    {
        int capacity = ParseCapacityOrDefault();
        int upperBound = GetCapacityUpperBound();
        if (capacity >= upperBound)
        {
            return;
        }

        CapacityText = Math.Min(upperBound, capacity + 1).ToString(LocalizationService.Culture);
    }

    [RelayCommand(CanExecute = nameof(CanDecrementCapacity))]
    private void DecrementCapacity()
    {
        int capacity = ParseCapacityOrDefault();
        if (capacity <= 1)
        {
            return;
        }

        CapacityText = (capacity - 1).ToString(LocalizationService.Culture);
    }

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private async Task ConfirmAsync()
    {
        if (!Validate(showMessage: true) || _model is null)
        {
            return;
        }

        if (!int.TryParse(_model.Id.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int areaId) || areaId <= 0)
        {
            ValidationMessageText = LocalizationService.GetString("StartSessionInvalidAreaIdMessage");
            OnPropertyChanged(nameof(HasValidationMessage));
            return;
        }

        int guestCount = ParseCapacityOrDefault();

        int? memberIdForStart = null;
        if (IsMember
            && int.TryParse(
                SelectedMemberId?.Trim(),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int memberNumericId)
            && memberNumericId > 0)
        {
            memberIdForStart = memberNumericId;
        }

        AreaSessionStartResult start;
        try
        {
            start = await _managementApiService.StartAreaSessionAsync(areaId, guestCount, memberIdForStart);
        }
        catch (Exception ex)
        {
            SessionFlowDebugLog.Append("StartSession.StartAreaSessionAsync", ex);
            ValidationMessageText = string.Format(
                LocalizationService.Culture,
                LocalizationService.GetString("StartSessionServerErrorMessage"),
                ex.Message);
            OnPropertyChanged(nameof(HasValidationMessage));
            FireDebugNotificationAsync(
                LocalizationService.GetString("StartSessionUnexpectedErrorTitle"),
                BuildUserFacingDetail(ex));
            return;
        }

        try
        {
            // Snapshot before closing: CloseRequested may dispose this VM (see StartSessionDialog.Closed),
            // and mutating Status synchronously can dispose the DetailedAvailable card that hosted this dialog —
            // both in the same stack and crash WinUI. Close first, then apply model on the next dispatcher tick.
            AreaModel area = _model;
            Member? selectedMember = ResolveSelectedMember();
            bool asMember = IsMember;
            string? memberIdValue = asMember ? selectedMember?.Id : null;
            string customerNameValue = asMember ? selectedMember?.FullName ?? string.Empty : string.Empty;
            string phoneValue = asMember ? selectedMember?.PhoneNumber ?? string.Empty : string.Empty;

            CloseRequestedInternal?.Invoke();

            DispatcherQueue? dispatcher = DispatcherQueue.GetForCurrentThread();
            if (dispatcher is not null)
            {
                // Two-phase enqueue: first tick lets ContentDialog/Hide finish teardown; second tick applies
                // AreaModel.Status (which swaps card VMs). Single tick still races with WinUI native cleanup → freeze/crash.
                dispatcher.TryEnqueue(() =>
                {
                    dispatcher.TryEnqueue(() =>
                    {
                        try
                        {
                            ApplySessionStartToArea(area, start, guestCount, memberIdValue, customerNameValue, phoneValue);
                        }
                        catch (Exception ex)
                        {
                            SessionFlowDebugLog.Append("StartSession.ApplySessionStartToArea", ex);
                            FireDebugNotificationAsync(
                                LocalizationService.GetString("StartSessionUnexpectedErrorTitle"),
                                BuildUserFacingDetail(ex));
                        }
                    });
                });
            }
            else
            {
                try
                {
                    ApplySessionStartToArea(area, start, guestCount, memberIdValue, customerNameValue, phoneValue);
                }
                catch (Exception ex)
                {
                    SessionFlowDebugLog.Append("StartSession.ApplySessionStartToArea(sync)", ex);
                    FireDebugNotificationAsync(
                        LocalizationService.GetString("StartSessionUnexpectedErrorTitle"),
                        BuildUserFacingDetail(ex));
                }
            }
        }
        catch (Exception ex)
        {
            SessionFlowDebugLog.Append("StartSession.ConfirmAsync.afterApi", ex);
            ValidationMessageText = string.Format(
                LocalizationService.Culture,
                LocalizationService.GetString("StartSessionUnexpectedErrorMessage"),
                ex.Message,
                SessionFlowDebugLog.LogFilePath);
            OnPropertyChanged(nameof(HasValidationMessage));
            FireDebugNotificationAsync(
                LocalizationService.GetString("StartSessionUnexpectedErrorTitle"),
                BuildUserFacingDetail(ex));
        }
    }

    private void FireDebugNotificationAsync(string title, string message)
    {
        _ = _notificationService.SendAsync(title, message, NotificationType.Error);
    }

    private static string BuildUserFacingDetail(Exception ex)
    {
        const int maxLen = 3500;
        string full = ex.ToString();
        if (full.Length > maxLen)
        {
            full = full[..maxLen] + "\n…";
        }

        return $"{full}\n\nLog file:\n{SessionFlowDebugLog.LogFilePath}";
    }

    private static void ApplySessionStartToArea(
        AreaModel model,
        AreaSessionStartResult start,
        int guestCount,
        string? memberId,
        string customerName,
        string phone)
    {
        model.MemberId = memberId;
        model.CustomerName = customerName;
        model.PhoneNumber = phone;
        model.CheckInDateTime = null;
        model.Capacity = guestCount;
        model.ActiveSessionId = start.SessionId.ToString(CultureInfo.InvariantCulture);
        model.StartTime = start.StartTimeUtc;
        model.IsSessionPaused = false;
        model.SessionPausedAt = null;
        model.SessionPausedDuration = TimeSpan.Zero;
        model.TotalAmount = 0m;
        model.PendingSessionLines.Clear();
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequestedInternal?.Invoke();
    }

    private void LoadMembers()
    {
        try
        {
            var members = _memberRepository.GetAllAsync().GetAwaiter().GetResult()
                .Where(member => member is not null && member.IsActive)
                .OrderBy(member => member.FullName)
                .ToList();

            Members.Clear();
            foreach (Member member in members)
            {
                Members.Add(member);
            }
        }
        catch (Exception ex)
        {
            SessionFlowDebugLog.Append("StartSession.LoadMembers", ex);
            Members.Clear();
            ValidationMessageText = string.Format(
                LocalizationService.Culture,
                LocalizationService.GetString("StartSessionLoadMembersFailedMessage"),
                ex.Message);
            OnPropertyChanged(nameof(HasValidationMessage));
            FireDebugNotificationAsync(
                LocalizationService.GetString("StartSessionUnexpectedErrorTitle"),
                BuildUserFacingDetail(ex));
        }
    }

    private bool CanDecrementCapacity() => ParseCapacityOrDefault() > 1;

    private bool CanIncrementCapacity() => ParseCapacityOrDefault() < GetCapacityUpperBound();

    private int ParseCapacityOrDefault()
    {
        return int.TryParse(CapacityText?.Trim(), out int capacity) && capacity > 0
            ? ClampCapacity(capacity)
            : 1;
    }

    private int ClampCapacity(int capacity)
    {
        return Math.Clamp(capacity, 1, GetCapacityUpperBound());
    }

    private int GetCapacityUpperBound()
    {
        return _model?.MaxCapacity > 0
            ? Math.Max(1, _model.MaxCapacity)
            : int.MaxValue;
    }

    private bool Validate(bool showMessage)
    {
        string validationMessage = string.Empty;

        if (!int.TryParse(CapacityText?.Trim(), out int capacity) || capacity <= 0)
        {
            validationMessage = LocalizationService.GetString("StartSessionDialogCapacityInvalidText");
        }
        else if (_model is not null && _model.MaxCapacity > 0 && capacity > _model.MaxCapacity)
        {
            validationMessage = string.Format(
                LocalizationService.Culture,
                LocalizationService.GetString("StartSessionDialogCapacityLimitText"),
                _model.MaxCapacity);
        }
        else if (IsMember && ResolveSelectedMember() is null)
        {
            validationMessage = LocalizationService.GetString("StartSessionDialogMemberRequiredText");
        }

        if (showMessage)
        {
            ValidationMessageText = validationMessage;
            OnPropertyChanged(nameof(HasValidationMessage));
        }

        return string.IsNullOrWhiteSpace(validationMessage);
    }

    private void UpdateValidationState()
    {
        ValidationMessageText = string.Empty;
        ConfirmCommand.NotifyCanExecuteChanged();
        DecrementCapacityCommand.NotifyCanExecuteChanged();
        IncrementCapacityCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanConfirm));
        OnPropertyChanged(nameof(HasValidationMessage));
        OnPropertyChanged(nameof(ShowMemberSelection));
    }

    partial void OnIsMemberChanged(bool value)
    {
        if (!value)
        {
            SelectedMemberId = null;
        }

        UpdateValidationState();
    }

    partial void OnSelectedMemberIdChanged(string? value) => UpdateValidationState();

    private Member? ResolveSelectedMember()
    {
        if (string.IsNullOrWhiteSpace(SelectedMemberId))
        {
            return null;
        }

        return Members.FirstOrDefault(m => string.Equals(m.Id, SelectedMemberId, StringComparison.Ordinal));
    }

    partial void OnCapacityTextChanged(string value) => UpdateValidationState();
}
