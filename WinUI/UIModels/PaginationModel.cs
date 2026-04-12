using CommunityToolkit.Mvvm.ComponentModel;

namespace WinUI.UIModels;

public partial class PaginationModel : ObservableObject
{
    [ObservableProperty]
    public partial int CurrentPage { get; set; } = 1;

    [ObservableProperty]
    public partial int TotalItems { get; set; } = 32;

    [ObservableProperty]
    public partial int PageSize { get; set; } = 8;

    [ObservableProperty]
    public partial int MaxVisiblePageButtons { get; set; } = 4;
}
