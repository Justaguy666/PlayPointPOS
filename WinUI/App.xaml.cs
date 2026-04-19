using System;
using System.Diagnostics;
using System.IO;
using Application.Areas;
using Application.Games;
using Application.Members;
using Application.Products;
using Application.Transactions;
using Application.Interfaces;
using Application.Navigation;
using Application.Services;
using Application.Services.Games;
using Application.Services.Members;
using Application.Services.Products;
using Application.Services.Areas;
using Application.Services.Transactions;
using Domain.Entities;
using Infrastructure.Repositories.Mock;
using Infrastructure.Services.Navigation;
using Infrastructure.Services.Members;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;
using WinUI.Services;
using WinUI.Services.Factories;
using WinUI.ViewModels.Dialogs;
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
                    var defaultPreferences = new LocalizationPreferences
                    {
                        Language = context.Configuration["AppSettings:Language"] ?? LocalizationPreferences.DefaultLanguage,
                        Currency = context.Configuration["AppSettings:Currency"] ?? LocalizationPreferences.DefaultCurrency,
                        TimeZone = context.Configuration["AppSettings:Timezone"] ?? LocalizationPreferences.DefaultTimeZone,
                        DateFormat = context.Configuration["AppSettings:DateFormat"] ?? LocalizationPreferences.DefaultDateFormat,
                    };

                    services.AddSingleton<Converters.CurrencyConverter>();
                    services.AddSingleton<ILocalizationService>(sp =>
                        new WinUILocalizationService(
                            sp.GetRequiredService<Converters.CurrencyConverter>(),
                            defaultPreferences.Language,
                            defaultPreferences.Currency,
                            defaultPreferences.TimeZone));
                    services.AddSingleton<IAppInfoService, WinUIAppInfoService>();
                    services.AddSingleton<IDateTimeService, Infrastructure.Services.DateTimeService>();
                    services.AddSingleton<INavigationService, WinUINavigationService>();
                    //services.AddSingleton<IAnalyticsService, Infrastructure.Services.Analytics.AnalyticsService>();

                    var configurationService = new Infrastructure.Services.ConfigurationService(configPath, defaultPreferences);
                    services.AddSingleton<Infrastructure.Services.ConfigurationService>(configurationService);
                    services.AddSingleton<IConfigurationService>(configurationService);
                    services.AddSingleton<ILocalizationPreferencesService>(configurationService);
                    services.AddSingleton<IAreaCatalogService, Infrastructure.Services.Areas.MockAreaCatalogService>();
                    services.AddSingleton<IAreaFilterService, AreaFilterService>();
                    services.AddSingleton<IAreaSessionService, AreaSessionService>();
                    services.AddSingleton<IGameTypeCatalogService, Infrastructure.Services.Games.MockGameTypeCatalogService>();
                    services.AddSingleton<IGameCatalogService, Infrastructure.Services.Games.MockGameCatalogService>();
                    services.AddSingleton<IGameFilterService, GameFilterService>();
                    services.AddSingleton<IProductCatalogService, Infrastructure.Services.Products.MockProductCatalogService>();
                    services.AddSingleton<IProductFilterService, ProductFilterService>();
                    services.AddSingleton<IMembershipRankCatalogService, MockMembershipRankCatalogService>();
                    services.AddSingleton<IMemberCatalogService, MockMemberCatalogService>();
                    services.AddSingleton<IMemberFilterService, MemberFilterService>();
                    services.AddSingleton<ITransactionCatalogService, Infrastructure.Services.Transactions.MockTransactionCatalogService>();
                    services.AddSingleton<ITransactionFilterService, TransactionFilterService>();

                    services.AddSingleton<Infrastructure.Services.Notification.ToastNotificationService>();
                    services.AddSingleton<INotificationService>(sp => sp.GetRequiredService<Infrastructure.Services.Notification.ToastNotificationService>());

                    services.AddSingleton<IPasswordHasher, Infrastructure.Services.PasswordHasher>();

                    services.AddSingleton<Func<string, object?, ContentDialog?>>(provider => (dialogKey, parameter) =>
                    {
                        return dialogKey switch
                        {
                            "Config" => new ConfigDialog(provider.GetRequiredService<ViewModels.Dialogs.ConfigViewModel>()),
                            "Register" => new RegisterDialog(provider.GetRequiredService<ViewModels.Dialogs.RegisterViewModel>()),
                            "Login" => new LoginDialog(provider.GetRequiredService<ViewModels.Dialogs.LoginViewModel>()),
                            "ForgotPassword" => new ForgotPasswordDialog(provider.GetRequiredService<ViewModels.Dialogs.ForgotPasswordViewModel>()),
                            "Otp" => CreateOtpDialog(provider, parameter),
                            "Reservation" => CreateReservationDialog(provider, parameter),
                            "AreaFilter" => CreateAreaFilterDialog(provider, parameter),
                            "GameFilter" => CreateGameFilterDialog(provider, parameter),
                            "Payment" => CreatePaymentDialog(provider, parameter),
                            "StartSession" => CreateStartSessionDialog(provider, parameter),
                            "Area" => CreateAreaDialog(provider, parameter),
                            "Game" => CreateGameDialog(provider, parameter),
                            "Product" => CreateProductDialog(provider, parameter),
                            "ProductFilter" => CreateProductFilterDialog(provider, parameter),
                            "Member" => CreateMemberDialog(provider, parameter),
                            "MemberFilter" => CreateMemberFilterDialog(provider, parameter),
                            "MembershipPackage" => CreateMembershipPackageDialog(provider, parameter),
                            "MembershipPackageEdit" => CreateMembershipPackageEditDialog(provider, parameter),
                            "GameType" => CreateGameTypeDialog(provider, parameter),
                            "GoalKpi" => CreateGoalKpiDialog(provider, parameter),
                            "TransactionDetail" => CreateTransactionDetailDialog(provider, parameter),
                            "TransactionFilter" => CreateTransactionFilterDialog(provider, parameter),
                            _ => null
                        };
                    });
                    services.AddSingleton<IDialogService, WinUIDialogService>();

                    services.AddTransient<Application.UseCases.Auth.LoginUserUseCase>();
                    services.AddTransient<Application.UseCases.Auth.RegisterUserUseCase>();
                    services.AddTransient<Application.UseCases.Pagination.BuildPaginationStateUseCase>();
                    //services.AddTransient<Application.UseCases.Analytics.GetChartAnalyticsUseCase>()

                    services.AddSingleton<IRepository<Account>, MockAccountRepository>();
                    services.AddSingleton<IRepository<BoardGame>, MockRepository<BoardGame>>();
                    services.AddSingleton<IRepository<Product>, MockRepository<Product>>();
                    services.AddSingleton<IRepository<Member>>(_ => CreateMockMemberRepository());
                    services.AddSingleton<IRepository<Membership>, MockRepository<Membership>>();

                    //services.AddSingleton<IAnalyticsProvider, Infrastructure.Services.Analytics.MockAnalyticsProvider>();

                    services.AddSingleton<ViewModels.MainViewModel>();
                    services.AddSingleton<ViewModels.UserControls.NavbarControlViewModel>();
                    services.AddSingleton<ViewModels.UserControls.NotificationControlViewModel>();
                    services.AddSingleton<ViewModels.UserControls.SearchControlViewModel>();
                    services.AddSingleton<StatCardControlViewModelFactory>();
                    services.AddSingleton<PopularCardControlViewModelFactory>();
                    services.AddSingleton<AreaModelFactory>();
                    services.AddSingleton<GameModelFactory>();
                    services.AddSingleton<GameCardControlViewModelFactory>();
                    services.AddSingleton<ProductModelFactory>();
                    services.AddSingleton<ProductCardControlViewModelFactory>();
                    services.AddSingleton<MemberModelFactory>();
                    services.AddSingleton<MemberCardControlViewModelFactory>();
                    services.AddSingleton<TransactionModelFactory>();
                    services.AddSingleton<TransactionCardControlViewModelFactory>();
                    services.AddSingleton<SummarizedAvailableCardViewModelFactory>();
                    services.AddSingleton<SummarizedReservedCardViewModelFactory>();
                    services.AddSingleton<SummarizedRentedCardViewModelFactory>();
                    services.AddSingleton<AreaManagementCardViewModelFactory>();
                    
                    services.AddTransient<ViewModels.Dialogs.ConfigViewModel>();
                    services.AddTransient<ViewModels.Dialogs.LoginViewModel>();
                    services.AddTransient<ViewModels.Dialogs.RegisterViewModel>();
                    services.AddTransient<ViewModels.Dialogs.ForgotPasswordViewModel>();
                    services.AddTransient<ViewModels.Dialogs.OtpViewModel>();
                    services.AddTransient<ViewModels.Dialogs.Management.ReservationViewModel>();
                    services.AddTransient<ViewModels.Dialogs.Management.AreaFilterViewModel>();
                    services.AddTransient<ViewModels.Dialogs.Management.GameFilterViewModel>();
                    services.AddTransient<ViewModels.Dialogs.Management.ProductFilterViewModel>();
                    services.AddTransient<ViewModels.Dialogs.Management.MemberDialogViewModel>();
                    services.AddTransient<ViewModels.Dialogs.Management.MemberFilterViewModel>();
                    services.AddTransient<ViewModels.Dialogs.Management.MembershipPackageDialogViewModel>();
                    services.AddTransient<ViewModels.Dialogs.Management.MembershipPackageEditDialogViewModel>();
                    services.AddTransient<ViewModels.Dialogs.Management.GameTypeDialogViewModel>();
                    services.AddTransient<ViewModels.Dialogs.Management.PaymentViewModel>();
                    services.AddTransient<ViewModels.Dialogs.Management.TransactionDetailDialogViewModel>();
                    services.AddTransient<ViewModels.Dialogs.Management.TransactionFilterDialogViewModel>();
                    services.AddTransient<ViewModels.Dialogs.StartSessionViewModel>();
                    services.AddTransient<ViewModels.Dialogs.Dashboard.GoalKpiDialogViewModel>();
                    services.AddTransient<ViewModels.UserControls.Dashboard.RevenueChartControlViewModel>();
                    services.AddTransient<ViewModels.UserControls.Dashboard.QuickStatsControlViewModel>();
                    services.AddTransient<ViewModels.UserControls.Dashboard.GoalProgressControlViewModel>();
                    services.AddTransient<ViewModels.UserControls.Dashboard.TrendingListControlViewModel>();
                    services.AddTransient<ViewModels.UserControls.PaginationControlViewModel>();
                    services.AddTransient<ViewModels.UserControls.Settings.ShopInformationCardControlViewModel>();
                    services.AddTransient<ViewModels.UserControls.Settings.GeneralSettingsCardControlViewModel>();
                    services.AddTransient<ViewModels.Pages.DashboardPageViewModel>();
                    services.AddTransient<ViewModels.Pages.AreaManagementPageViewModel>();
                    services.AddTransient<ViewModels.Pages.GameManagementPageViewModel>();
                    services.AddTransient<ViewModels.Pages.MemberManagementPageViewModel>();
                    services.AddTransient<ViewModels.Pages.ProductManagementPageViewModel>();
                    services.AddTransient<ViewModels.Pages.TransactionManagementPageViewModel>();
                    services.AddTransient<ViewModels.Pages.StartingPageViewModel>();
                    services.AddTransient<ViewModels.Pages.SettingsPageViewModel>();

                    services.AddSingleton<MainWindow>();
                    services.AddTransient<Views.Pages.StartingPage>();
                    services.AddTransient<Views.Pages.DashboardPage>();
                    services.AddTransient<Views.Pages.AreaManagementPage>();
                    services.AddTransient<Views.Pages.GameManagementPage>();
                    services.AddTransient<Views.Pages.ProductManagementPage>();
                    services.AddTransient<Views.Pages.MemberManagementPage>();
                    services.AddTransient<Views.Pages.TransactionManagementPage>();
                    services.AddTransient<Views.Pages.SettingsPage>();
                })
                .Build();

            NavigationMap.Register<Application.Navigation.Requests.NavigateToStarting, Views.Pages.StartingPage>();
            NavigationMap.Register<Application.Navigation.Requests.NavigateToDashboard, Views.Pages.DashboardPage>();
            NavigationMap.Register<Application.Navigation.Requests.NavigateToAreaManagement, Views.Pages.AreaManagementPage>();
            NavigationMap.Register<Application.Navigation.Requests.NavigateToGameManagement, Views.Pages.GameManagementPage>();
            NavigationMap.Register<Application.Navigation.Requests.NavigateToProductManagement, Views.Pages.ProductManagementPage>();
            NavigationMap.Register<Application.Navigation.Requests.NavigateToMemberManagement, Views.Pages.MemberManagementPage>();
            NavigationMap.Register<Application.Navigation.Requests.NavigateToTransactionHistory, Views.Pages.TransactionManagementPage>();
            NavigationMap.Register<Application.Navigation.Requests.NavigateToSettings, Views.Pages.SettingsPage>();

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
            var configurationService = Host!.Services.GetRequiredService<IConfigurationService>();
            await configurationService.LoadAsync();

            var preferencesService = Host.Services.GetRequiredService<ILocalizationPreferencesService>();
            var localizationService = Host.Services.GetRequiredService<ILocalizationService>();
            LocalizationPreferences preferences = preferencesService.Preferences;
            localizationService.ApplyPreferences(preferences.Language, preferences.Currency, preferences.TimeZone);

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

    private static ContentDialog CreateOtpDialog(IServiceProvider provider, object? parameter)
    {
        var viewModel = provider.GetRequiredService<ViewModels.Dialogs.OtpViewModel>();
        viewModel.Configure(parameter as OtpDialogRequest);
        return new OtpDialog(viewModel);
    }

    private static ContentDialog CreateReservationDialog(IServiceProvider provider, object? parameter)
    {
        var request = parameter switch
        {
            ViewModels.Dialogs.Management.ReservationDialogRequest reservationRequest => reservationRequest,
            UIModels.Management.AreaModel areaModel => new ViewModels.Dialogs.Management.ReservationDialogRequest
            {
                Mode = UIModels.Enums.UpsertDialogMode.Add,
                Model = areaModel,
            },
            _ => new ViewModels.Dialogs.Management.ReservationDialogRequest(),
        };

        var viewModel = new ViewModels.Dialogs.Management.ReservationViewModel(
            provider.GetRequiredService<ILocalizationService>(),
            provider.GetRequiredService<ILocalizationPreferencesService>(),
            provider.GetRequiredService<IRepository<Member>>(),
            provider.GetRequiredService<IDialogService>(),
            provider.GetRequiredService<AreaModelFactory>(),
            request.Mode);

        return new Views.Dialogs.Management.ReservationDialog(viewModel, request);
    }

    private static ContentDialog CreateStartSessionDialog(IServiceProvider provider, object? parameter)
    {
        var viewModel = provider.GetRequiredService<ViewModels.Dialogs.StartSessionViewModel>();
        return new StartSessionDialog(viewModel, parameter as UIModels.Management.AreaModel);
    }

    private static ContentDialog CreateAreaFilterDialog(IServiceProvider provider, object? parameter)
    {
        var viewModel = provider.GetRequiredService<ViewModels.Dialogs.Management.AreaFilterViewModel>();
        return new Views.Dialogs.Management.AreaFilterDialog(
            viewModel,
            parameter as ViewModels.Dialogs.Management.AreaFilterDialogRequest);
    }

    private static ContentDialog CreatePaymentDialog(IServiceProvider provider, object? parameter)
    {
        var viewModel = provider.GetRequiredService<ViewModels.Dialogs.Management.PaymentViewModel>();
        return new Views.Dialogs.Management.PaymentDialog(viewModel, parameter as UIModels.Management.AreaModel);
    }

    private static ContentDialog CreateAreaDialog(IServiceProvider provider, object? parameter)
    {
        var request = parameter as ViewModels.Dialogs.Management.AreaDialogRequest;
        var viewModel = new ViewModels.Dialogs.Management.AreaDialogViewModel(
            provider.GetRequiredService<ILocalizationService>(),
            provider.GetRequiredService<IDialogService>(),
            provider.GetRequiredService<AreaModelFactory>(),
            request?.Mode ?? UIModels.Enums.UpsertDialogMode.Add);
        return new Views.Dialogs.Management.AreaDialog(viewModel, request);
    }

    private static ContentDialog CreateGameFilterDialog(IServiceProvider provider, object? parameter)
    {
        var viewModel = provider.GetRequiredService<ViewModels.Dialogs.Management.GameFilterViewModel>();
        return new Views.Dialogs.Management.GameFilterDialog(
            viewModel,
            parameter as ViewModels.Dialogs.Management.GameFilterDialogRequest);
    }

    private static ContentDialog CreateGameTypeDialog(IServiceProvider provider, object? parameter)
    {
        var viewModel = provider.GetRequiredService<ViewModels.Dialogs.Management.GameTypeDialogViewModel>();
        if (parameter is ViewModels.Dialogs.Management.GameTypeDialogRequest request)
        {
            viewModel.Configure(request);
        }
        return new Views.Dialogs.Management.GameTypeDialog(viewModel);
    }

    private static ContentDialog CreateGameDialog(IServiceProvider provider, object? parameter)
    {
        var request = parameter as ViewModels.Dialogs.Management.GameDialogRequest;
        var viewModel = new ViewModels.Dialogs.Management.GameDialogViewModel(
            provider.GetRequiredService<ILocalizationService>(),
            provider.GetRequiredService<IDialogService>(),
            provider.GetRequiredService<GameModelFactory>(),
            request?.Mode ?? UIModels.Enums.UpsertDialogMode.Add);
        return new Views.Dialogs.Management.GameDialog(
            viewModel,
            request,
            provider.GetRequiredService<MainWindow>());
    }

    private static ContentDialog CreateProductFilterDialog(IServiceProvider provider, object? parameter)
    {
        var viewModel = provider.GetRequiredService<ViewModels.Dialogs.Management.ProductFilterViewModel>();
        return new Views.Dialogs.Management.ProductFilterDialog(
            viewModel,
            parameter as ViewModels.Dialogs.Management.ProductFilterDialogRequest);
    }

    private static ContentDialog CreateTransactionDetailDialog(IServiceProvider provider, object? parameter)
    {
        var viewModel = provider.GetRequiredService<ViewModels.Dialogs.Management.TransactionDetailDialogViewModel>();
        return new Views.Dialogs.Management.TransactionDetailDialog(
            viewModel,
            parameter as ViewModels.Dialogs.Management.TransactionDetailDialogRequest);
    }

    private static ContentDialog CreateTransactionFilterDialog(IServiceProvider provider, object? parameter)
    {
        var viewModel = provider.GetRequiredService<ViewModels.Dialogs.Management.TransactionFilterDialogViewModel>();
        return new Views.Dialogs.Management.TransactionFilterDialog(
            viewModel,
            parameter as ViewModels.Dialogs.Management.TransactionFilterDialogRequest);
    }

    private static ContentDialog CreateProductDialog(IServiceProvider provider, object? parameter)
    {
        var request = parameter as ViewModels.Dialogs.Management.ProductDialogRequest;
        var viewModel = new ViewModels.Dialogs.Management.ProductDialogViewModel(
            provider.GetRequiredService<ILocalizationService>(),
            provider.GetRequiredService<IDialogService>(),
            provider.GetRequiredService<ProductModelFactory>(),
            request?.Mode ?? UIModels.Enums.UpsertDialogMode.Add);
        return new Views.Dialogs.Management.ProductDialog(
            viewModel,
            request,
            provider.GetRequiredService<MainWindow>());
    }

    private static ContentDialog CreateMemberDialog(IServiceProvider provider, object? parameter)
    {
        var request = parameter as ViewModels.Dialogs.Management.MemberDialogRequest;
        var viewModel = new ViewModels.Dialogs.Management.MemberDialogViewModel(
            provider.GetRequiredService<ILocalizationService>(),
            provider.GetRequiredService<IDialogService>(),
            provider.GetRequiredService<MemberModelFactory>(),
            request?.Mode ?? UIModels.Enums.UpsertDialogMode.Add);
        return new Views.Dialogs.Management.MemberDialog(viewModel, request);
    }

    private static ContentDialog CreateMemberFilterDialog(IServiceProvider provider, object? parameter)
    {
        var viewModel = provider.GetRequiredService<ViewModels.Dialogs.Management.MemberFilterViewModel>();
        return new Views.Dialogs.Management.MemberFilterDialog(
            viewModel,
            parameter as ViewModels.Dialogs.Management.MemberFilterDialogRequest);
    }

    private static ContentDialog CreateMembershipPackageDialog(IServiceProvider provider, object? parameter)
    {
        var viewModel = provider.GetRequiredService<ViewModels.Dialogs.Management.MembershipPackageDialogViewModel>();
        return new Views.Dialogs.Management.MembershipPackageDialog(
            viewModel,
            parameter as ViewModels.Dialogs.Management.MembershipPackageDialogRequest
                ?? throw new InvalidOperationException("Membership package dialog request is required."));
    }

    private static ContentDialog CreateMembershipPackageEditDialog(IServiceProvider provider, object? parameter)
    {
        var viewModel = provider.GetRequiredService<ViewModels.Dialogs.Management.MembershipPackageEditDialogViewModel>();
        return new Views.Dialogs.Management.MembershipPackageEditDialog(
            viewModel,
            parameter as ViewModels.Dialogs.Management.MembershipPackageEditDialogRequest
                ?? throw new InvalidOperationException("Membership package edit dialog request is required."));
    }

    private static ContentDialog CreateGoalKpiDialog(IServiceProvider provider, object? parameter)
    {
        var viewModel = provider.GetRequiredService<ViewModels.Dialogs.Dashboard.GoalKpiDialogViewModel>();
        return new Views.Dialogs.Dashboard.GoalKpiDialog(
            viewModel,
            parameter as ViewModels.Dialogs.Dashboard.GoalKpiDialogRequest);
    }

    private static IRepository<Member> CreateMockMemberRepository()
    {
        var repository = new MockRepository<Member>();

        repository.AddAsync(new Member
        {
            FullName = "Nguyen Minh Anh",
            PhoneNumber = "0901 234 567",
        }).GetAwaiter().GetResult();

        repository.AddAsync(new Member
        {
            FullName = "Tran Hoang Nam",
            PhoneNumber = "0912 345 678",
        }).GetAwaiter().GetResult();

        repository.AddAsync(new Member
        {
            FullName = "Le Thu Ha",
            PhoneNumber = "0987 654 321",
        }).GetAwaiter().GetResult();

        return repository;
    }
}
