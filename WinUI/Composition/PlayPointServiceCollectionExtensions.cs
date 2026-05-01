using System;
using Application.Areas;
using Application.Games;
using Application.Interfaces;
using Application.Members;
using Application.Navigation;
using Application.Products;
using Application.Services;
using Application.Services.Areas;
using Application.Services.Games;
using Application.Services.Members;
using Application.Services.Products;
using Application.Services.Transactions;
using Application.Transactions;
using Domain.Entities;
using Infrastructure.Repositories.Mock;
using Infrastructure.Services.Members;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WinUI.Converters;
using WinUI.Services;
using WinUI.Services.Dialogs;
using WinUI.Services.Factories;
using WinUI.Services.Layout;
using WinUI.Services.Management;
using WinUI.ViewModels.Dialogs;
using WinUI.ViewModels.Dialogs.Dashboard;
using WinUI.ViewModels.Dialogs.Management;

namespace WinUI.Composition;

internal static class PlayPointServiceCollectionExtensions
{
    // WHY: Đây là Composition Root của toàn bộ ứng dụng. 
    // Tập trung đăng ký Dependency Injection (DI) tại đây giúp WinUI project không cần reference trực tiếp 
    // đến cụ thể các implementations ở tầng Infrastructure (tuân thủ Dependency Inversion).
    public static IServiceCollection AddPlayPointServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string configPath)
    {
        var defaultPreferences = new LocalizationPreferences
        {
            Language = configuration["AppSettings:Language"] ?? LocalizationPreferences.DefaultLanguage,
            Currency = configuration["AppSettings:Currency"] ?? LocalizationPreferences.DefaultCurrency,
            TimeZone = configuration["AppSettings:Timezone"] ?? LocalizationPreferences.DefaultTimeZone,
            DateFormat = configuration["AppSettings:DateFormat"] ?? LocalizationPreferences.DefaultDateFormat,
        };

        services.AddCoreServices(defaultPreferences, configPath);
        services.AddRepositories();
        services.AddUseCases();
        services.AddFactories();
        services.AddViewModels();
        services.AddViews();
        services.AddNavigationRoutes();

        return services;
    }

    private static void AddCoreServices(
        this IServiceCollection services,
        LocalizationPreferences defaultPreferences,
        string configPath)
    {
        services.AddSingleton<CurrencyConverter>();
        services.AddSingleton<ILocalizationService>(sp =>
            new WinUILocalizationService(
                sp.GetRequiredService<CurrencyConverter>(),
                defaultPreferences.Language,
                defaultPreferences.Currency,
                defaultPreferences.TimeZone));
        services.AddSingleton<IAppInfoService, WinUIAppInfoService>();
        services.AddSingleton<IApplicationLifetimeService, WinUIApplicationLifetimeService>();
        services.AddSingleton<Func<MainWindow>>(sp => () => sp.GetRequiredService<MainWindow>());
        services.AddSingleton<Func<IFilePickerService>>(sp => () => sp.GetRequiredService<IFilePickerService>());
        services.AddSingleton<IFilePickerService, WinUIFilePickerService>();
        services.AddSingleton<IDateTimeService, Infrastructure.Services.DateTimeService>();
        services.AddSingleton<INavigationService, WinUINavigationService>();

        var configurationService = new Infrastructure.Services.ConfigurationService(configPath, defaultPreferences);
        services.AddSingleton(configurationService);
        services.AddSingleton<IConfigurationService>(configurationService);
        services.AddSingleton<ILocalizationPreferencesService>(configurationService);
        services.AddSingleton<IAreaCatalogService, Infrastructure.Services.Areas.MockAreaCatalogService>();
        services.AddSingleton<IAreaFilterService, AreaFilterService>();
        services.AddSingleton<IAreaSessionService, AreaSessionService>();
        services.AddSingleton<IGameTypeCatalogService, Infrastructure.Services.Games.MockGameTypeCatalogService>();
        services.AddSingleton<IGameTypeManagementService, GameTypeManagementService>();
        services.AddSingleton<IGameCatalogService, Infrastructure.Services.Games.MockGameCatalogService>();
        services.AddSingleton<IGameFilterService, GameFilterService>();
        services.AddSingleton<IProductCatalogService, Infrastructure.Services.Products.MockProductCatalogService>();
        services.AddSingleton<IProductFilterService, ProductFilterService>();
        services.AddSingleton<IMembershipRankCatalogService, MockMembershipRankCatalogService>();
        services.AddSingleton<IMembershipRankManagementService, MembershipRankManagementService>();
        services.AddSingleton<IMemberLookupService, RepositoryMemberLookupService>();
        services.AddSingleton<IMemberCatalogService, MockMemberCatalogService>();
        services.AddSingleton<IMemberFilterService, MemberFilterService>();
        services.AddSingleton<ITransactionCatalogService, Infrastructure.Services.Transactions.MockTransactionCatalogService>();
        services.AddSingleton<ITransactionFilterService, TransactionFilterService>();

        services.AddSingleton<Infrastructure.Services.Notification.ToastNotificationService>();
        services.AddSingleton<INotificationService>(sp =>
            sp.GetRequiredService<Infrastructure.Services.Notification.ToastNotificationService>());
        services.AddSingleton<INotificationEventSource>(sp =>
            sp.GetRequiredService<Infrastructure.Services.Notification.ToastNotificationService>());

        services.AddSingleton<IPasswordHasher, Infrastructure.Services.PasswordHasher>();
        services.AddSingleton<IResponsiveLayoutService, ResponsiveLayoutService>();
        services.AddDialogBuilders();
        services.AddDialogServiceFactories();
        services.AddSingleton<IDialogFactory, DialogRegistry>();
        services.AddSingleton<IDialogService, WinUIDialogService>();
    }

    private static void AddDialogBuilders(this IServiceCollection services)
    {
        services.AddDialogViewModelFactory<ConfigViewModel>();
        services.AddDialogViewModelFactory<RegisterViewModel>();
        services.AddDialogViewModelFactory<LoginViewModel>();
        services.AddDialogViewModelFactory<ForgotPasswordViewModel>();
        services.AddDialogViewModelFactory<OtpViewModel>();
        services.AddDialogViewModelFactory<StartSessionViewModel>();
        services.AddDialogViewModelFactory<AreaFilterViewModel>();
        services.AddDialogViewModelFactory<PaymentViewModel>();
        services.AddDialogViewModelFactory<GameFilterViewModel>();
        services.AddDialogViewModelFactory<GameTypeDialogViewModel>();
        services.AddDialogViewModelFactory<ProductFilterViewModel>();
        services.AddDialogViewModelFactory<MemberFilterViewModel>();
        services.AddDialogViewModelFactory<MembershipPackageDialogViewModel>();
        services.AddDialogViewModelFactory<MembershipPackageEditDialogViewModel>();
        services.AddDialogViewModelFactory<GoalKpiDialogViewModel>();
        services.AddDialogViewModelFactory<TransactionDetailDialogViewModel>();
        services.AddDialogViewModelFactory<TransactionFilterDialogViewModel>();

        services.AddDialogBuilder<ConfigDialogBuilder>();
        services.AddDialogBuilder<RegisterDialogBuilder>();
        services.AddDialogBuilder<LoginDialogBuilder>();
        services.AddDialogBuilder<ForgotPasswordDialogBuilder>();
        services.AddDialogBuilder<OtpDialogBuilder>();
        services.AddDialogBuilder<ReservationDialogBuilder>();
        services.AddDialogBuilder<StartSessionDialogBuilder>();
        services.AddDialogBuilder<AreaFilterDialogBuilder>();
        services.AddDialogBuilder<PaymentDialogBuilder>();
        services.AddDialogBuilder<AreaDialogBuilder>();
        services.AddDialogBuilder<GameFilterDialogBuilder>();
        services.AddDialogBuilder<GameTypeDialogBuilder>();
        services.AddDialogBuilder<GameDialogBuilder>();
        services.AddDialogBuilder<ProductFilterDialogBuilder>();
        services.AddDialogBuilder<ProductDialogBuilder>();
        services.AddDialogBuilder<MemberDialogBuilder>();
        services.AddDialogBuilder<MemberFilterDialogBuilder>();
        services.AddDialogBuilder<MembershipPackageDialogBuilder>();
        services.AddDialogBuilder<MembershipPackageEditDialogBuilder>();
        services.AddDialogBuilder<GoalKpiDialogBuilder>();
        services.AddDialogBuilder<TransactionDetailDialogBuilder>();
        services.AddDialogBuilder<TransactionFilterDialogBuilder>();
    }

    private static void AddDialogBuilder<TBuilder>(this IServiceCollection services)
        where TBuilder : class, IDialogDefinition
    {
        services.AddSingleton<IDialogDefinition, TBuilder>();
    }

    private static void AddDialogServiceFactories(this IServiceCollection services)
    {
        services.AddSingleton<Func<IDialogService>>(sp => () => sp.GetRequiredService<IDialogService>());
    }

    private static void AddDialogViewModelFactory<TViewModel>(this IServiceCollection services)
        where TViewModel : notnull
    {
        services.AddSingleton<Func<TViewModel>>(sp => () => sp.GetRequiredService<TViewModel>());
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IRepository<Account>, MockAccountRepository>();
        services.AddSingleton<IRepository<BoardGame>, MockRepository<BoardGame>>();
        services.AddSingleton<IRepository<Product>, MockRepository<Product>>();
        services.AddSingleton<IRepository<Member>>(_ => CreateMockMemberRepository());
        services.AddSingleton<IRepository<Membership>, MockRepository<Membership>>();
    }

    private static void AddUseCases(this IServiceCollection services)
    {
        services.AddTransient<Application.UseCases.Auth.LoginUserUseCase>();
        services.AddTransient<Application.UseCases.Auth.RegisterUserUseCase>();
        services.AddTransient<Application.UseCases.Pagination.BuildPaginationStateUseCase>();
    }

    private static void AddFactories(this IServiceCollection services)
    {
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
        services.AddSingleton<GameDraftFactory>();
        services.AddSingleton<ProductDraftFactory>();
        services.AddSingleton<MemberDraftFactory>();
        services.AddSingleton<AreaDraftFactory>();
        services.AddSingleton<GameManagementDialogCoordinator>();
        services.AddSingleton<ProductManagementDialogCoordinator>();
        services.AddSingleton<MemberManagementDialogCoordinator>();
        services.AddSingleton<TransactionManagementDialogCoordinator>();
        services.AddSingleton<AreaManagementDialogCoordinator>();
    }

    private static void AddViewModels(this IServiceCollection services)
    {
        services.AddSingleton<ViewModels.MainViewModel>();
        services.AddSingleton<ViewModels.UserControls.NavbarControlViewModel>();
        services.AddSingleton<ViewModels.UserControls.NotificationControlViewModel>();

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
    }

    private static void AddViews(this IServiceCollection services)
    {
        services.AddSingleton<MainWindow>();
        services.AddTransient<Views.Pages.StartingPage>();
        services.AddTransient<Views.Pages.DashboardPage>();
        services.AddTransient<Views.Pages.AreaManagementPage>();
        services.AddTransient<Views.Pages.GameManagementPage>();
        services.AddTransient<Views.Pages.ProductManagementPage>();
        services.AddTransient<Views.Pages.MemberManagementPage>();
        services.AddTransient<Views.Pages.TransactionManagementPage>();
        services.AddTransient<Views.Pages.SettingsPage>();
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
