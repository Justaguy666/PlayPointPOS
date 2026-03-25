using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;
using Application.Navigation;
using Application.Services;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Services.Navigation;
using Infrastructure.Repositories.Mock;
using WinUI.Views.Dialogs;

namespace WinUI;

public partial class App : Microsoft.UI.Xaml.Application
{
    public static IHost? Host { get; private set; }
    private Window? _window;

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Package.Current.InstalledLocation.Path);
                config.AddJsonFile("Configurations\\appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Register Services (Infrastructure implementations)
                services.AddSingleton<ILocalizationService, Infrastructure.Services.LocalizationService>();
                services.AddSingleton<IAppInfoService, Infrastructure.Services.AppInfoService>();
                services.AddSingleton<INavigationService, Infrastructure.Services.Navigation.NavigationService>();

                // Register ConfigurationService (reads/writes config.json)
                var configPath = Path.Combine(Package.Current.InstalledLocation.Path, "Configurations", "config.json");
                services.AddSingleton<IConfigurationService>(new Infrastructure.Services.ConfigurationService(configPath));

                // Register NotificationService (toast)
                services.AddSingleton<Infrastructure.Services.Notification.ToastNotificationService>();
                services.AddSingleton<INotificationService>(sp => sp.GetRequiredService<Infrastructure.Services.Notification.ToastNotificationService>());

                // Register DialogService with factory for dialog creation
                services.AddSingleton<Func<string, ContentDialog?>>(provider => dialogKey =>
                {
                    return dialogKey switch
                    {
                        "Config" => new ConfigDialog(),
                        "Register" => new RegisterDialog(),
                        "Login" => new LoginDialog(),
                        _ => null
                    };
                });
                services.AddSingleton<IDialogService, Infrastructure.Services.Dialog.DialogService>();

                // Register Repositories (swap implementation here)
                // Option 1: Mock (in-memory — development/testing)
                services.AddSingleton<IRepository<Account>, MockAccountRepository>();
                services.AddSingleton<IRepository<BoardGame>, MockRepository<BoardGame>>();
                services.AddSingleton<IRepository<Drink>, MockRepository<Drink>>();
                services.AddSingleton<IRepository<Food>, MockRepository<Food>>();
                services.AddSingleton<IRepository<Membership>, MockRepository<Membership>>();

                // Option 2: JSON File (local storage) — uncomment to switch
                // var dataDir = Path.Combine(Package.Current.InstalledLocation.Path, "Data");
                // services.AddSingleton<IRepository<Account>>(new JsonFileRepository<Account>(Path.Combine(dataDir, "accounts.json")));
                // services.AddSingleton<IRepository<BoardGame>>(new JsonFileRepository<BoardGame>(Path.Combine(dataDir, "boardgames.json")));
                // services.AddSingleton<IRepository<Drink>>(new JsonFileRepository<Drink>(Path.Combine(dataDir, "drinks.json")));
                // services.AddSingleton<IRepository<Food>>(new JsonFileRepository<Food>(Path.Combine(dataDir, "foods.json")));
                // services.AddSingleton<IRepository<Membership>>(new JsonFileRepository<Membership>(Path.Combine(dataDir, "memberships.json")));

                // Register ViewModels
                services.AddTransient<ViewModels.StartingViewModel>();
                services.AddTransient<ViewModels.MainViewModel>();
                services.AddTransient<ViewModels.Dialogs.ConfigViewModel>();
                services.AddTransient<ViewModels.Dialogs.LoginViewModel>();
                services.AddTransient<ViewModels.Dialogs.RegisterViewModel>();

                // Register Views
                services.AddTransient<MainWindow>();
                services.AddTransient<Views.Pages.StartingPage>();
                services.AddTransient<Views.Pages.DashboardPage>();
            })
            .Build();

        // Register navigation mappings
        NavigationMap.Register<Application.Navigation.Requests.NavigateToStarting, Views.Pages.StartingPage>();
        NavigationMap.Register<Application.Navigation.Requests.NavigateToDashboard, Views.Pages.DashboardPage>();
        NavigationMap.Register<Application.Navigation.Requests.NavigateToTableManagement, Views.Pages.TableManagementPage>();
        NavigationMap.Register<Application.Navigation.Requests.NavigateToGameManagement, Views.Pages.GameManagementPage>();
        NavigationMap.Register<Application.Navigation.Requests.NavigateToFoodManagement, Views.Pages.FoodManagementPage>();
        NavigationMap.Register<Application.Navigation.Requests.NavigateToMemberManagement, Views.Pages.MemberManagementPage>();
        NavigationMap.Register<Application.Navigation.Requests.NavigateToTransactionHistory, Views.Pages.TransactionHistoryPage>();
        NavigationMap.Register<Application.Navigation.Requests.NavigateToSettings, Views.Pages.SettingsPagePage>();

        // Set language from configuration
        var configuration = Host.Services.GetRequiredService<IConfiguration>();
        var language = configuration["AppSettings:Language"] ?? "vi-VN";
        Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = language;
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Load saved configuration
        await Host!.Services.GetRequiredService<IConfigurationService>().LoadAsync();

        _window = Host?.Services.GetRequiredService<MainWindow>();
        _window?.Activate();

        if (_window?.Content is FrameworkElement rootElement)
        {
            Host?.Services.GetRequiredService<IDialogService>().Initialize(rootElement);
        }

        // Hook toast notification event to MainWindow
        if (_window is MainWindow mainWindow)
        {
            var toastService = Host?.Services.GetRequiredService<Infrastructure.Services.Notification.ToastNotificationService>();
            if (toastService != null)
            {
                toastService.NotificationRequested += (title, message, type) =>
                {
                    mainWindow.DispatcherQueue.TryEnqueue(() => 
                    {
                        mainWindow.ShowNotification(title, message, type);
                    });
                };
            }
        }
    }
}
