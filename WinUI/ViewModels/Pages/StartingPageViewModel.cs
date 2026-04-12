using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinUI.UIModels;
using WinUI.UIModels.Enums;

namespace WinUI.ViewModels.Pages;

public partial class StartingPageViewModel : LocalizedViewModelBase
{
    private readonly IAppInfoService _appInfo;
    private readonly IDialogService _dialogService;
    private readonly IConfigurationService _configService;

    [ObservableProperty]
    public partial string WelcomeText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AppVersionText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CopyrightText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<MenuItemModel> MenuItems { get; set; } = new();

    public IAsyncRelayCommand<MenuItemModel?> OnMenuItemSelectedCommand { get; }

    public StartingPageViewModel(
        ILocalizationService localizationService,
        IAppInfoService appInfo,
        IDialogService dialogService,
        IConfigurationService configService)
        : base(localizationService)
    {
        _appInfo = appInfo;
        _dialogService = dialogService;
        _configService = configService;

        OnMenuItemSelectedCommand = new AsyncRelayCommand<MenuItemModel?>(OnMenuItemSelectedAsync);

        LoadMenuItems();
        RefreshLocalizedText();
    }

    protected override void RefreshLocalizedText()
    {
        WelcomeText = LocalizationService.GetString("WelcomeText");

        AppVersionText = string.Format(
            LocalizationService.GetString("AppVersionText"),
            _appInfo.GetAppVersion()
        );

        CopyrightText = LocalizationService.GetString("CopyrightText");
        UpdateMenu();
    }

    private void LoadMenuItems()
    {
        var config = new MenuItemModel
        {
            LabelResourceKey = "ConfigMenuItemText",
            Icon = IconKind.Config,
            DialogKey = "Config",
            HideWhenConfigured = true,
            OnMenuItemSelectedCommand = OnMenuItemSelectedCommand,
        };
        var register = new MenuItemModel
        {
            LabelResourceKey = "RegisterMenuItemText",
            Icon = IconKind.Register,
            DialogKey = "Register",
            RequiresConfig = true,
            OnMenuItemSelectedCommand = OnMenuItemSelectedCommand,
        };
        var login = new MenuItemModel
        {
            LabelResourceKey = "LoginMenuItemText",
            Icon = IconKind.Login,
            DialogKey = "Login",
            RequiresConfig = true,
            OnMenuItemSelectedCommand = OnMenuItemSelectedCommand,
        };
        var exit = new MenuItemModel
        {
            LabelResourceKey = "ExitMenuItemText",
            Icon = IconKind.Exit,
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
            item.Label = LocalizationService.GetString(item.LabelResourceKey);
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
            bool isConfirmed = await _dialogService.ShowConfirmationAsync(
                titleKey: "ConfirmExitTitle",
                messageKey: "ConfirmExitMessage",
                confirmButtonTextKey: "ConfirmExitButton",
                cancelButtonTextKey: "CancelButtonText"
            );

            if (isConfirmed)
            {
                Environment.Exit(0);
            }
            return;
        }

        await _dialogService.ShowDialogAsync(selectedItem.DialogKey);

        UpdateMenuVisibility();
    }
}
