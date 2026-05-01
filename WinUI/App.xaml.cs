using System;
using System.Diagnostics;
using System.IO;
using Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;
using WinUI.Composition;
using WinUI.Services.Dialogs;

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
                    services.AddPlayPointServices(context.Configuration, configPath);
                })
                .Build();
            var configuration = Host.Services.GetRequiredService<IConfiguration>();
            var language = configuration["AppSettings:Language"] ?? LocalizationPreferences.DefaultLanguage;
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
            var configurationService = Host!.Services.GetRequiredService<IConfigurationService>();
            await configurationService.LoadAsync();

            var preferencesService = Host.Services.GetRequiredService<ILocalizationPreferencesService>();
            var localizationService = Host.Services.GetRequiredService<ILocalizationService>();
            LocalizationPreferences preferences = preferencesService.Preferences;
            localizationService.ApplyPreferences(preferences.Language, preferences.Currency, preferences.TimeZone);

            _window = Host.Services.GetRequiredService<MainWindow>();
            _window.Activate();

            if (_window.Content is FrameworkElement rootElement)
            {
                Host.Services.GetRequiredService<IDialogService>().Initialize(rootElement);
            }

            if (_window is MainWindow mainWindow)
            {
                var notificationEvents = Host.Services.GetRequiredService<INotificationEventSource>();
                notificationEvents.NotificationRequested += (title, message, type)
                    => mainWindow.DispatcherQueue.TryEnqueue(() => mainWindow.ShowNotification(title, message, type));
            }
        }
        catch (Exception ex)
        {
            LogStartupException("OnLaunched failed", ex);
            throw;
        }
    }

    // WHY: Hàm này giải quyết vấn đề tìm file config appsettings.json.
    // Nếu ứng dụng chạy dạng Packaged (MSIX), root sẽ nằm ở InstalledLocation.
    // Nếu chạy dạng Unpackaged (Debug F5 trong Visual Studio), root phải dò lùi lại từ bin folder.
    private static string ResolveContentRoot()
    {
        // WORKAROUND: TryGetInstalledLocationPath có thể văng Exception nếu ứng dụng không chạy dạng Packaged,
        // do đó bắt buộc phải bọc try-catch bên trong hàm đó.
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
            "Configurations");

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
                "Logs");

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
