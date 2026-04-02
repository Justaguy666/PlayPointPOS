namespace WinUI.ViewModels.UserControls.Dashboard;

public sealed record PopularCardItemData(
    int Rank,
    string Name,
    int ActivityCount,
    decimal Amount);
