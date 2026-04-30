using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Application.Areas;
using Application.Members;
using Application.Services;
using Application.Services.Areas;
using Application.Services.Members;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinUI.UIModels.Management;
using WinUI.ViewModels;

namespace WinUI.ViewModels.Dialogs;

public partial class StartSessionViewModel : LocalizedViewModelBase
{
    private readonly IMemberLookupService _memberLookupService;
    private readonly IAreaSessionService _areaSessionService;
    private AreaModel? _model;
    private event Action? CloseRequestedInternal;

    public StartSessionViewModel(
        ILocalizationService localizationService,
        IMemberLookupService memberLookupService,
        IAreaSessionService areaSessionService)
        : base(localizationService)
    {
        _memberLookupService = memberLookupService ?? throw new ArgumentNullException(nameof(memberLookupService));
        _areaSessionService = areaSessionService ?? throw new ArgumentNullException(nameof(areaSessionService));
        Members = [];
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
    public partial MemberLookupItem? SelectedMember { get; set; }

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

    public ObservableCollection<MemberLookupItem> Members { get; }

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
        CapacityText = _areaSessionService.ClampCapacity(initialCapacity, model?.MaxCapacity ?? 0).ToString(LocalizationService.Culture);
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

        _areaSessionService.StartSession(
            _model,
            new StartAreaSessionRequest
            {
                MemberId = IsMember ? SelectedMember?.Id : null,
                CustomerName = IsMember ? SelectedMember?.FullName ?? string.Empty : string.Empty,
                PhoneNumber = IsMember ? SelectedMember?.PhoneNumber ?? string.Empty : string.Empty,
                Capacity = ParseCapacityOrDefault(),
            },
            DateTime.UtcNow);

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
        Members.Clear();
        foreach (MemberLookupItem member in _memberLookupService.GetActiveMembers())
        {
            Members.Add(member);
        }
    }

    private bool CanDecrementCapacity() => ParseCapacityOrDefault() > 1;

    private bool CanIncrementCapacity() => ParseCapacityOrDefault() < GetCapacityUpperBound();

    private int ParseCapacityOrDefault()
    {
        return int.TryParse(CapacityText?.Trim(), out int capacity) && capacity > 0
            ? _areaSessionService.ClampCapacity(capacity, _model?.MaxCapacity ?? 0)
            : 1;
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

    partial void OnSelectedMemberChanged(MemberLookupItem? value) => UpdateValidationState();

    partial void OnCapacityTextChanged(string value) => UpdateValidationState();
}
