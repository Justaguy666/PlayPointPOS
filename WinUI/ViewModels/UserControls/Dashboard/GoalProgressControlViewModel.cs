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
    public partial int RevenueTargetProgress { get; set; } = 285;

    [ObservableProperty]
    public partial int RevenueTargetValue { get; set; } = 350;

    [ObservableProperty]
    public partial string RevenueTargetPercent { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int CustomerTargetProgress { get; set; } = 156;

    [ObservableProperty]
    public partial int CustomerTargetValue { get; set; } = 200;

    [ObservableProperty]
    public partial string CustomerTargetPercent { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int MemberTargetProgress { get; set; } = 23;

    [ObservableProperty]
    public partial int MemberTargetValue { get; set; } = 30;

    [ObservableProperty]
    public partial string MemberTargetPercent { get; set; } = string.Empty;

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
        Title = LocalizationService.GetString("GoalProgressCardTitle");
        RevenueTargetLabel = LocalizationService.GetString("GoalProgressCardRevenueTargetLabel");
        CustomerTargetLabel = LocalizationService.GetString("GoalProgressCardCustomerTargetLabel");
        MemberTargetLabel = LocalizationService.GetString("GoalProgressCardMemberTargetLabel");

        RevenueTargetPercent = FormatCompletion(RevenueTargetProgress, RevenueTargetValue);
        CustomerTargetPercent = FormatCompletion(CustomerTargetProgress, CustomerTargetValue);
        MemberTargetPercent = FormatCompletion(MemberTargetProgress, MemberTargetValue);
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
