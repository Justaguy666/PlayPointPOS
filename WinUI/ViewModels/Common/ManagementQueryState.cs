namespace WinUI.ViewModels.Common;

public sealed class ManagementQueryState<TFilter, TSort>
{
    public string SearchText { get; set; } = string.Empty;

    public TFilter Filter { get; set; }

    public TSort Sort { get; set; }

    public ManagementQueryState(TFilter filter, TSort sort)
    {
        Filter = filter;
        Sort = sort;
    }
}
