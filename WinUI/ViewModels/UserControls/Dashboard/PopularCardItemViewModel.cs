namespace WinUI.ViewModels.UserControls.Dashboard;

public sealed class PopularCardItemViewModel
{
    public PopularCardItemViewModel(
        int rank,
        string name,
        string activityLabel,
        string amountLabel)
    {
        Rank = rank;
        Name = name;
        ActivityLabel = activityLabel;
        AmountLabel = amountLabel;
    }

    public int Rank { get; }

    public string Name { get; }

    public string ActivityLabel { get; }

    public string AmountLabel { get; }
}
