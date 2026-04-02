namespace WinUI.ViewModels.UserControls.Dashboard;

public sealed class PopularCardItemViewModel
{
    public PopularCardItemViewModel(
        int rank,
        string name,
        string activityText,
        string amountText)
    {
        Rank = rank;
        Name = name;
        ActivityText = activityText;
        AmountText = amountText;
    }

    public int Rank { get; }

    public string Name { get; }

    public string ActivityText { get; }

    public string AmountText { get; }
}
