using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels;

namespace WinUI.ViewModels.UserControls.Dashboard;

public partial class GoalProgressControlViewModel : LocalizedViewModelBase
{
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

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

    public GoalProgressControlViewModel(ILocalizationService localizationService)
        : base(localizationService)
    {
        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        Title = LocalizationService.GetString("GoalProgressTitle");
        RevenueTargetLabel = LocalizationService.GetString("GoalProgressRevenueTargetLabel");
        CustomerTargetLabel = LocalizationService.GetString("GoalProgressCustomerTargetLabel");
        MemberTargetLabel = LocalizationService.GetString("GoalProgressMemberTargetLabel");

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
