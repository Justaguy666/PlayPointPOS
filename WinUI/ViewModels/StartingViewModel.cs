using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Application.Services;
using Application.Navigation;
using Application.Navigation.Requests;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinUI.UIModels;

namespace WinUI.ViewModels;

public partial class StartingViewModel : ObservableObject
{
    private readonly ILocalizationService _loc;
    private readonly IAppInfoService _appInfo;
    private readonly IDialogService _dialogService;
    private readonly IConfigurationService _configService;

    [ObservableProperty]
    public partial string WelcomeTextDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AppVersionDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CopyrightDisplay { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<MenuItemModel> MenuItems { get; set; } = new();

    public IAsyncRelayCommand<MenuItemModel?> OnMenuItemSelectedCommand { get; }

    public StartingViewModel(
        ILocalizationService loc,
        IAppInfoService appInfo,
        IDialogService dialogService,
        IConfigurationService configService)
    {
        _loc = loc;
        _appInfo = appInfo;
        _dialogService = dialogService;
        _configService = configService;

        OnMenuItemSelectedCommand = new AsyncRelayCommand<MenuItemModel?>(OnMenuItemSelectedAsync);

        _loc.LanguageChanged += UpdateTexts;
        _loc.LanguageChanged += UpdateMenu;

        UpdateTexts();
        LoadMenuItems();
    }

    private void UpdateTexts()
    {
        WelcomeTextDisplay = _loc.GetString("WelcomeText");

        AppVersionDisplay = string.Format(
            _loc.GetString("AppVersionText"),
            _appInfo.GetAppVersion()
        );

        CopyrightDisplay = _loc.GetString("CopyrightText");
    }

    private void LoadMenuItems()
    {
        var config = new MenuItemModel
        {
            ResourceKey = "ConfigMenuItemText",
            Icon = "\uE713",
            DialogKey = "Config",
            HideWhenConfigured = true,
            OnMenuItemSelectedCommand = OnMenuItemSelectedCommand,
        };
        var register = new MenuItemModel
        {
            ResourceKey = "RegisterMenuItemText",
            Icon = "\uE8FA",
            DialogKey = "Register",
            RequiresConfig = true,
            OnMenuItemSelectedCommand = OnMenuItemSelectedCommand,
        };
        var login = new MenuItemModel
        {
            ResourceKey = "LoginMenuItemText",
            Icon = "\uE72E",
            DialogKey = "Login",
            RequiresConfig = true,
            OnMenuItemSelectedCommand = OnMenuItemSelectedCommand,
        };
        var exit = new MenuItemModel
        {
            ResourceKey = "ExitMenuItemText",
            Icon = "\uF3B1",
            IsExit = true,
            OnMenuItemSelectedCommand = OnMenuItemSelectedCommand,
        };

        MenuItems.Add(config);
        MenuItems.Add(register);
        MenuItems.Add(login);
        MenuItems.Add(exit);
        UpdateMenu();
        UpdateMenuVisibility();
    }

    private void UpdateMenu()
    {
        foreach (var item in MenuItems)
        {
            item.DisplayText = _loc.GetString(item.ResourceKey);
        }
    }

    private void UpdateMenuVisibility()
    {
        var isConfigured = _configService.IsConfigured;
        foreach (var item in MenuItems)
        {
            if (item.RequiresConfig)
            {
                item.IsVisible = isConfigured;
            }
            else if (item.HideWhenConfigured)
            {
                item.IsVisible = !isConfigured;
            }
        }
    }

    private async Task OnMenuItemSelectedAsync(MenuItemModel? selectedItem)
    {
        if (selectedItem == null) return;

        if (selectedItem.IsExit)
        {
            Environment.Exit(0);
            return;
        }

        await _dialogService.ShowDialogAsync(selectedItem.DialogKey);

        UpdateMenuVisibility();
    }
}
