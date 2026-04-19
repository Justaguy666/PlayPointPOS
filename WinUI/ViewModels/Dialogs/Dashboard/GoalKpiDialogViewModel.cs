using System;
using System.Globalization;
using System.Threading.Tasks;
using Application.Services;
using CommunityToolkit.Mvvm.Input;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels;

namespace WinUI.ViewModels.Dialogs.Dashboard;

public partial class GoalKpiDialogViewModel : LocalizedViewModelBase
{
    private Func<int, int, int, Task>? _onSubmittedAsync;
    private event Action? CloseRequestedInternal;

    private IconState _icon = new();
    private string _titleText = string.Empty;
    private string _revenueGoalLabelText = string.Empty;
    private string _customerGoalLabelText = string.Empty;
    private string _memberGoalLabelText = string.Empty;
    private string _revenueGoalText = string.Empty;
    private string _customerGoalText = string.Empty;
    private string _memberGoalText = string.Empty;
    private string _goalPlaceholderText = string.Empty;
    private string _resetButtonText = string.Empty;
    private string _applyButtonText = string.Empty;
    private string _closeTooltipText = string.Empty;
    private string _errorMessage = string.Empty;
    private string _revenueGoalInput = string.Empty;
    private string _customerGoalInput = string.Empty;
    private string _memberGoalInput = string.Empty;

    public GoalKpiDialogViewModel(ILocalizationService localizationService)
        : base(localizationService)
    {
        Icon = new IconState
        {
            Kind = IconKind.Target,
            Size = 24,
            AlwaysFilled = true,
        };

        CloseCommand = new RelayCommand(Close);
        ResetCommand = new RelayCommand(Reset);
        ApplyCommand = new AsyncRelayCommand(ApplyAsync, () => CanApply);

        RefreshLocalizedText();
    }

    public IconState Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }

    public string TitleText
    {
        get => _titleText;
        set => SetProperty(ref _titleText, value);
    }

    public string RevenueGoalLabelText
    {
        get => _revenueGoalLabelText;
        set => SetProperty(ref _revenueGoalLabelText, value);
    }

    public string CustomerGoalLabelText
    {
        get => _customerGoalLabelText;
        set => SetProperty(ref _customerGoalLabelText, value);
    }

    public string MemberGoalLabelText
    {
        get => _memberGoalLabelText;
        set => SetProperty(ref _memberGoalLabelText, value);
    }

    public string RevenueGoalText
    {
        get => _revenueGoalText;
        set => SetProperty(ref _revenueGoalText, value);
    }

    public string CustomerGoalText
    {
        get => _customerGoalText;
        set => SetProperty(ref _customerGoalText, value);
    }

    public string MemberGoalText
    {
        get => _memberGoalText;
        set => SetProperty(ref _memberGoalText, value);
    }

    public string GoalPlaceholderText
    {
        get => _goalPlaceholderText;
        set => SetProperty(ref _goalPlaceholderText, value);
    }

    public string ResetButtonText
    {
        get => _resetButtonText;
        set => SetProperty(ref _resetButtonText, value);
    }

    public string ApplyButtonText
    {
        get => _applyButtonText;
        set => SetProperty(ref _applyButtonText, value);
    }

    public string CloseTooltipText
    {
        get => _closeTooltipText;
        set => SetProperty(ref _closeTooltipText, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public string RevenueGoalInput
    {
        get => _revenueGoalInput;
        set
        {
            if (SetProperty(ref _revenueGoalInput, value))
            {
                NotifyCanApplyStateChanged();
                ClearError();
            }
        }
    }

    public string CustomerGoalInput
    {
        get => _customerGoalInput;
        set
        {
            if (SetProperty(ref _customerGoalInput, value))
            {
                NotifyCanApplyStateChanged();
                ClearError();
            }
        }
    }

    public string MemberGoalInput
    {
        get => _memberGoalInput;
        set
        {
            if (SetProperty(ref _memberGoalInput, value))
            {
                NotifyCanApplyStateChanged();
                ClearError();
            }
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool CanApply => TryParsePositiveInt(RevenueGoalInput, out _)
                            && TryParsePositiveInt(CustomerGoalInput, out _)
                            && TryParsePositiveInt(MemberGoalInput, out _);

    public IRelayCommand CloseCommand { get; }

    public IRelayCommand ResetCommand { get; }

    public IAsyncRelayCommand ApplyCommand { get; }

    public event Action? CloseRequested
    {
        add => CloseRequestedInternal += value;
        remove => CloseRequestedInternal -= value;
    }

    public void Configure(GoalKpiDialogRequest? request)
    {
        _onSubmittedAsync = request?.OnSubmittedAsync;

        RevenueGoalText = request?.RevenueGoalValue.ToString(LocalizationService.Culture) ?? "0";
        CustomerGoalText = request?.CustomerGoalValue.ToString(LocalizationService.Culture) ?? "0";
        MemberGoalText = request?.MemberGoalValue.ToString(LocalizationService.Culture) ?? "0";

        RevenueGoalInput = RevenueGoalText;
        CustomerGoalInput = CustomerGoalText;
        MemberGoalInput = MemberGoalText;
        ErrorMessage = string.Empty;
    }

    protected override void RefreshLocalizedText()
    {
        TitleText = LocalizationService.GetString("GoalProgressEditDialogTitleText");
        RevenueGoalLabelText = LocalizationService.GetString("GoalProgressRevenueTargetLabel");
        CustomerGoalLabelText = LocalizationService.GetString("GoalProgressCustomerTargetLabel");
        MemberGoalLabelText = LocalizationService.GetString("GoalProgressMemberTargetLabel");
        GoalPlaceholderText = LocalizationService.GetString("GoalProgressEditDialogGoalPlaceholderText");
        ResetButtonText = LocalizationService.GetString("GameDialogResetButtonText");
        ApplyButtonText = LocalizationService.GetString("GameDialogApplyButtonText");
        CloseTooltipText = LocalizationService.GetString("CloseTooltipText");
    }

    private void Close()
    {
        CloseRequestedInternal?.Invoke();
    }

    private void Reset()
    {
        RevenueGoalInput = RevenueGoalText;
        CustomerGoalInput = CustomerGoalText;
        MemberGoalInput = MemberGoalText;
        ErrorMessage = string.Empty;
    }

    private async Task ApplyAsync()
    {
        if (!TryParsePositiveInt(RevenueGoalInput, out int revenueGoal)
            || !TryParsePositiveInt(CustomerGoalInput, out int customerGoal)
            || !TryParsePositiveInt(MemberGoalInput, out int memberGoal))
        {
            ErrorMessage = LocalizationService.GetString("GoalProgressEditDialogInvalidValueText");
            return;
        }

        ErrorMessage = string.Empty;

        if (_onSubmittedAsync is not null)
        {
            await _onSubmittedAsync(revenueGoal, customerGoal, memberGoal);
        }

        CloseRequestedInternal?.Invoke();
    }

    private void NotifyCanApplyStateChanged()
    {
        OnPropertyChanged(nameof(CanApply));
        ApplyCommand.NotifyCanExecuteChanged();
    }

    private void ClearError()
    {
        if (!string.IsNullOrWhiteSpace(ErrorMessage))
        {
            ErrorMessage = string.Empty;
        }
    }

    private bool TryParsePositiveInt(string? text, out int value)
    {
        const NumberStyles styles = NumberStyles.Integer;
        string trimmedText = text?.Trim() ?? string.Empty;

        bool success =
            int.TryParse(trimmedText, styles, LocalizationService.Culture, out value)
            || int.TryParse(trimmedText, styles, CultureInfo.InvariantCulture, out value);

        return success && value > 0;
    }
}
