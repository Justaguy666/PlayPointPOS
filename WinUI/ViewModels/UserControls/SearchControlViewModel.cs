using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using Application.Services;
using WinUI.UIModels.Enums;

namespace WinUI.ViewModels.UserControls;

public partial class SearchControlViewModel : LocalizedViewModelBase
{
    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IconKind IconKind { get; set; } = IconKind.Search;

    public IRelayCommand<string> SearchCommand { get; }
    public IRelayCommand ClearCommand { get; }

    public SearchControlViewModel(ILocalizationService localizationService) 
        : base(localizationService)
    {
        SearchCommand = new RelayCommand<string>(ExecuteSearch);
        ClearCommand = new RelayCommand(ExecuteClear);
        
        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        PlaceholderText = LocalizationService.GetString("SearchPlaceholderText");
    }

    private void ExecuteSearch(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return;
        }

        // Handle search logic here
        System.Diagnostics.Debug.WriteLine($"Search executed with query: {query}");
    }

    private void ExecuteClear()
    {
        SearchText = string.Empty;
    }
}
