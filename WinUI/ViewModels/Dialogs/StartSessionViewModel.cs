using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Entities;
using WinUI.UIModels.Management;
using WinUI.ViewModels;

namespace WinUI.ViewModels.Dialogs;

public partial class StartSessionViewModel : LocalizedViewModelBase
{
    private readonly IRepository<Member> _memberRepository;
    private AreaModel? _model;
    private event Action? CloseRequestedInternal;

    public StartSessionViewModel(
        ILocalizationService localizationService,
        IRepository<Member> memberRepository)
        : base(localizationService)
    {
        _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConfirm))]
    public partial Member? SelectedMember { get; set; }

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
        SelectedMember = null;

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
    private Task ConfirmAsync()
    {
        if (!Validate(showMessage: true) || _model is null)
        {
            return Task.CompletedTask;
        }

        _model.MemberId = IsMember ? SelectedMember?.Id : null;
        _model.CustomerName = IsMember ? SelectedMember?.FullName ?? string.Empty : string.Empty;
        _model.PhoneNumber = IsMember ? SelectedMember?.PhoneNumber ?? string.Empty : string.Empty;
        _model.CheckInDateTime = null;
        _model.Capacity = ParseCapacityOrDefault();
        _model.StartTime = DateTime.UtcNow;
        _model.IsSessionPaused = false;
        _model.SessionPausedAt = null;
        _model.SessionPausedDuration = TimeSpan.Zero;
        _model.TotalAmount = 0m;
        _model.Status = Domain.Enums.PlayAreaStatus.Rented;

        CloseRequestedInternal?.Invoke();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequestedInternal?.Invoke();
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
        else if (IsMember && SelectedMember is null)
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

    partial void OnIsMemberChanged(bool value) => UpdateValidationState();

    partial void OnSelectedMemberChanged(Member? value) => UpdateValidationState();

    partial void OnCapacityTextChanged(string value) => UpdateValidationState();
}
