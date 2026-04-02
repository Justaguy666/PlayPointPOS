using System;
using System.Diagnostics;
using System.IO;
using Application.Interfaces;
using Application.Navigation;
using Application.Services;
using Domain.Entities;
using Infrastructure.Repositories.Mock;
using Infrastructure.Services.Navigation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;
using WinUI.Services;
using WinUI.Views.Dialogs;

namespace WinUI;

public partial class App : Microsoft.UI.Xaml.Application
{
    private const string AppDataFolderName = "PlayPointPOS";

    public static IHost? Host { get; private set; }
    private Window? _window;

    public App()
    {
        UnhandledException += App_UnhandledException;
        InitializeComponent();

        try
        {
            string contentRoot = ResolveContentRoot();
            string configPath = ResolveWritableConfigPath();

            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(contentRoot);
                    config.AddJsonFile("Configurations\\appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    string defaultLanguage = context.Configuration["AppSettings:Language"] ?? "en-US";
                    string defaultCurrency = context.Configuration["AppSettings:Currency"] ?? "VND";
                    string defaultTimeZone = context.Configuration["AppSettings:Timezone"] ?? "+7";

                    services.AddSingleton<Converters.CurrencyConverter>();
                    services.AddSingleton<ILocalizationService>(sp =>
                        new WinUILocalizationService(
                            sp.GetRequiredService<Converters.CurrencyConverter>(),
                            defaultLanguage,
                            defaultCurrency,
                            defaultTimeZone));
                    services.AddSingleton<IAppInfoService, WinUIAppInfoService>();
                    services.AddSingleton<IDateTimeService, Infrastructure.Services.DateTimeService>();
                    services.AddSingleton<INavigationService, WinUINavigationService>();
                    //services.AddSingleton<IAnalyticsService, Infrastructure.Services.Analytics.AnalyticsService>();

                    services.AddSingleton<IConfigurationService>(new Infrastructure.Services.ConfigurationService(configPath));

                    services.AddSingleton<Infrastructure.Services.Notification.ToastNotificationService>();
                    services.AddSingleton<INotificationService>(sp => sp.GetRequiredService<Infrastructure.Services.Notification.ToastNotificationService>());

                    services.AddSingleton<IPasswordHasher, Infrastructure.Services.PasswordHasher>();

                    services.AddSingleton<Func<string, ContentDialog?>>(provider => dialogKey =>
                    {
                        return dialogKey switch
                        {
                            "Config" => new ConfigDialog(provider.GetRequiredService<ViewModels.Dialogs.ConfigViewModel>()),
                            "Register" => new RegisterDialog(provider.GetRequiredService<ViewModels.Dialogs.RegisterViewModel>()),
                            "Login" => new LoginDialog(provider.GetRequiredService<ViewModels.Dialogs.LoginViewModel>()),
                            _ => null
                        };
                    });
                    services.AddSingleton<IDialogService, WinUIDialogService>();

                    services.AddTransient<Application.UseCases.Auth.LoginUserUseCase>();
                    services.AddTransient<Application.UseCases.Auth.RegisterUserUseCase>();
                    //services.AddTransient<Application.UseCases.Analytics.GetChartAnalyticsUseCase>()

                    services.AddSingleton<IRepository<Account>, MockAccountRepository>();
                    services.AddSingleton<IRepository<BoardGame>, MockRepository<BoardGame>>();
                    services.AddSingleton<IRepository<Product>, MockRepository<Product>>();
                    services.AddSingleton<IRepository<Membership>, MockRepository<Membership>>();

                    //services.AddSingleton<IAnalyticsProvider, Infrastructure.Services.Analytics.MockAnalyticsProvider>();

                    services.AddSingleton<ViewModels.MainViewModel>();
                    services.AddSingleton<ViewModels.UserControls.NavbarControlViewModel>();
                    services.AddSingleton<ViewModels.UserControls.NotificationControlViewModel>();
                    services.AddSingleton<StatCardControlViewModelFactory>();
                    services.AddSingleton<PopularCardControlViewModelFactory>();
                    
                    services.AddTransient<ViewModels.Dialogs.ConfigViewModel>();
                    services.AddTransient<ViewModels.Dialogs.LoginViewModel>();
                    services.AddTransient<ViewModels.Dialogs.RegisterViewModel>();
                    services.AddTransient<ViewModels.UserControls.Dashboard.ChartCardControlViewModel>();
                    services.AddTransient<ViewModels.UserControls.Dashboard.QuickStatsControlViewModel>();
                    services.AddTransient<ViewModels.UserControls.Dashboard.GoalProgressControlViewModel>();
                    services.AddTransient<ViewModels.UserControls.Dashboard.TrendingListControlViewModel>();
                    services.AddTransient<ViewModels.Pages.DashboardPageViewModel>();
                    services.AddTransient<ViewModels.Pages.StartingPageViewModel>();
                    services.AddTransient<ViewModels.Pages.SettingsPageViewModel>();

                    services.AddTransient<MainWindow>();
                    services.AddTransient<Views.Pages.StartingPage>();
                    services.AddTransient<Views.Pages.DashboardPage>();
                    services.AddTransient<Views.Pages.TableManagementPage>();
                    services.AddTransient<Views.Pages.GameManagementPage>();
                    services.AddTransient<Views.Pages.ProductManagementPage>();
                    services.AddTransient<Views.Pages.MemberManagementPage>();
                    services.AddTransient<Views.Pages.TransactionHistoryPage>();
                    services.AddTransient<Views.Pages.SettingsPagePage>();
                })
                .Build();

            NavigationMap.Register<Application.Navigation.Requests.NavigateToStarting, Views.Pages.StartingPage>();
            NavigationMap.Register<Application.Navigation.Requests.NavigateToDashboard, Views.Pages.DashboardPage>();
            NavigationMap.Register<Application.Navigation.Requests.NavigateToTableManagement, Views.Pages.TableManagementPage>();
            NavigationMap.Register<Application.Navigation.Requests.NavigateToGameManagement, Views.Pages.GameManagementPage>();
            NavigationMap.Register<Application.Navigation.Requests.NavigateToProductManagement, Views.Pages.ProductManagementPage>();
            NavigationMap.Register<Application.Navigation.Requests.NavigateToMemberManagement, Views.Pages.MemberManagementPage>();
            NavigationMap.Register<Application.Navigation.Requests.NavigateToTransactionHistory, Views.Pages.TransactionHistoryPage>();
            NavigationMap.Register<Application.Navigation.Requests.NavigateToSettings, Views.Pages.SettingsPagePage>();

            var configuration = Host.Services.GetRequiredService<IConfiguration>();
            var language = configuration["AppSettings:Language"] ?? "en-US";
            Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = language;
        }
        catch (Exception ex)
        {
            LogStartupException("App constructor failed", ex);
            throw;
        }
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            await Host!.Services.GetRequiredService<IConfigurationService>().LoadAsync();

            _window = Host?.Services.GetRequiredService<MainWindow>();
            _window?.Activate();

            if (_window?.Content is FrameworkElement rootElement)
            {
                Host?.Services.GetRequiredService<IDialogService>().Initialize(rootElement);
            }

            if (_window is MainWindow mainWindow)
            {
                var toastService = Host?.Services.GetRequiredService<Infrastructure.Services.Notification.ToastNotificationService>();
                if (toastService != null)
                {
                    toastService.NotificationRequested += (title, message, type)
                        => mainWindow.DispatcherQueue.TryEnqueue(() => mainWindow.ShowNotification(title, message, type));
                }
            }
        }
        catch (Exception ex)
        {
            LogStartupException("OnLaunched failed", ex);
            throw;
        }
    }

    private static string ResolveContentRoot()
    {
        string? packagedRoot = TryGetInstalledLocationPath();
        if (!string.IsNullOrWhiteSpace(packagedRoot))
        {
            return packagedRoot;
        }

        string currentDirectory = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(currentDirectory))
        {
            if (Directory.Exists(Path.Combine(currentDirectory, "Configurations")))
            {
                return currentDirectory;
            }

            currentDirectory = Directory.GetParent(currentDirectory)?.FullName ?? string.Empty;
        }

        return AppContext.BaseDirectory;
    }

    private static string ResolveWritableConfigPath()
    {
        string configDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppDataFolderName,
            "Configurations"
        );

        Directory.CreateDirectory(configDirectory);
        return Path.Combine(configDirectory, "config.json");
    }

    private static string? TryGetInstalledLocationPath()
    {
        try
        {
            return Package.Current.InstalledLocation.Path;
        }
        catch
        {
            return null;
        }
    }

    private static void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        LogStartupException($"UnhandledException: {e.Message}", e.Exception);
    }

    private static void LogStartupException(string context, Exception? exception)
    {
        try
        {
            string logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                AppDataFolderName,
                "Logs"
            );

            Directory.CreateDirectory(logDirectory);

            string logPath = Path.Combine(logDirectory, "startup.log");
            string details = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {context}{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}";

            File.AppendAllText(logPath, details);
            Debug.WriteLine(details);
        }
        catch
        {
            // Avoid masking the original exception when logging fails.
        }
    }
}
