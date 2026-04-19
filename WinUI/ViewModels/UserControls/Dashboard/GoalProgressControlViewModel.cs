using System;
using System.Threading.Tasks;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.Dialogs.Dashboard;
using WinUI.ViewModels;

namespace WinUI.ViewModels.UserControls.Dashboard;

public partial class GoalProgressControlViewModel : LocalizedViewModelBase
{
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    private string _editButtonText = string.Empty;
    public string EditButtonText
    {
        get => _editButtonText;
        set => SetProperty(ref _editButtonText, value);
    }

    private string _editTooltipText = string.Empty;
    public string EditTooltipText
    {
        get => _editTooltipText;
        set => SetProperty(ref _editTooltipText, value);
    }

    [ObservableProperty]
    public partial string RevenueTargetLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CustomerTargetLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string MemberTargetLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int RevenueTargetCurrentValue { get; set; } = 285;

    [ObservableProperty]
    public partial int RevenueTargetGoalValue { get; set; } = 350;

    [ObservableProperty]
    public partial string RevenueTargetCompletionText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int CustomerTargetCurrentValue { get; set; } = 156;

    [ObservableProperty]
    public partial int CustomerTargetGoalValue { get; set; } = 200;

    [ObservableProperty]
    public partial string CustomerTargetCompletionText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int MemberTargetCurrentValue { get; set; } = 23;

    [ObservableProperty]
    public partial int MemberTargetGoalValue { get; set; } = 30;

    [ObservableProperty]
    public partial string MemberTargetCompletionText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IconState IconState { get; set; } = new()
    {
        Kind = IconKind.Target,
        AlwaysFilled = true,
        Size = 24,
    };

    public IconState EditIconState { get; } = new()
    {
        Kind = IconKind.Update,
        AlwaysFilled = false,
        Size = 16,
    };

    public GoalProgressControlViewModel(ILocalizationService localizationService, IDialogService dialogService)
        : base(localizationService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        Title = LocalizationService.GetString("GoalProgressTitle");
        EditButtonText = LocalizationService.GetString("EditButtonText");
        EditTooltipText = LocalizationService.GetString("GoalProgressEditTooltipText");
        RevenueTargetLabel = LocalizationService.GetString("GoalProgressRevenueTargetLabel");
        CustomerTargetLabel = LocalizationService.GetString("GoalProgressCustomerTargetLabel");
        MemberTargetLabel = LocalizationService.GetString("GoalProgressMemberTargetLabel");

        RefreshCompletionTexts();
    }

    [RelayCommand]
    private Task EditGoalsAsync()
    {
        return _dialogService.ShowDialogAsync(
            "GoalKpi",
            new GoalKpiDialogRequest
            {
                RevenueGoalValue = RevenueTargetGoalValue,
                CustomerGoalValue = CustomerTargetGoalValue,
                MemberGoalValue = MemberTargetGoalValue,
                OnSubmittedAsync = HandleGoalsUpdatedAsync,
            });
    }

    private Task HandleGoalsUpdatedAsync(int revenueGoal, int customerGoal, int memberGoal)
    {
        RevenueTargetGoalValue = revenueGoal;
        CustomerTargetGoalValue = customerGoal;
        MemberTargetGoalValue = memberGoal;
        RefreshCompletionTexts();
        return Task.CompletedTask;
    }

    partial void OnRevenueTargetCurrentValueChanged(int value) => RevenueTargetCompletionText = FormatCompletion(value, RevenueTargetGoalValue);

    partial void OnRevenueTargetGoalValueChanged(int value) => RevenueTargetCompletionText = FormatCompletion(RevenueTargetCurrentValue, value);

    partial void OnCustomerTargetCurrentValueChanged(int value) => CustomerTargetCompletionText = FormatCompletion(value, CustomerTargetGoalValue);

    partial void OnCustomerTargetGoalValueChanged(int value) => CustomerTargetCompletionText = FormatCompletion(CustomerTargetCurrentValue, value);

    partial void OnMemberTargetCurrentValueChanged(int value) => MemberTargetCompletionText = FormatCompletion(value, MemberTargetGoalValue);

    partial void OnMemberTargetGoalValueChanged(int value) => MemberTargetCompletionText = FormatCompletion(MemberTargetCurrentValue, value);

    private void RefreshCompletionTexts()
    {
        RevenueTargetCompletionText = FormatCompletion(RevenueTargetCurrentValue, RevenueTargetGoalValue);
        CustomerTargetCompletionText = FormatCompletion(CustomerTargetCurrentValue, CustomerTargetGoalValue);
        MemberTargetCompletionText = FormatCompletion(MemberTargetCurrentValue, MemberTargetGoalValue);
    }

    private string FormatCompletion(int progress, int total)
    {
        double completionPercent = total == 0 ? 0 : (double)progress / total * 100;
        return string.Format(
            LocalizationService.Culture,
            LocalizationService.GetString("DashboardCompletionValueFormat"),
            completionPercent);
    }
}
