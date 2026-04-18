# WinUI Layer - Complete Architectural Analysis
## PlayPointPOS Desktop Application

---

## Table of Contents
1. [Project Overview](#project-overview)
2. [Project Structure](#project-structure)
3. [MVVM Architecture](#mvvm-architecture)
4. [Dependency Injection](#dependency-injection)
5. [Clean Architecture](#clean-architecture)
6. [Key Components Analysis](#key-components-analysis)
7. [Design Patterns](#design-patterns)
8. [Clean Code Practices](#clean-code-practices)
9. [Resources & Styling](#resources--styling)
10. [Configuration & Environment](#configuration--environment)
11. [Navigation System](#navigation-system)
12. [State Management](#state-management)

---

## Project Overview

**WinUI Project**: Desktop presentation layer for PlayPointPOS, a Point-of-Sale system for board game cafes.

**Technology Stack**:
- **Framework**: WinUI 3 (Windows App SDK 1.8)
- **.NET**: .NET 10.0 (Windows 10.0.19041.0 minimum)
- **MVVM Toolkit**: CommunityToolkit.Mvvm 8.4.1
- **DI Container**: Microsoft.Extensions.DependencyInjection
- **Configuration**: Microsoft.Extensions.Configuration
- **Password Hashing**: BCrypt.Net-Next 4.0.3
- **Graphics**: Microsoft.Graphics.Win2D 1.4.0

**Purpose**: 
- Full-featured POS UI for managing game areas, reservations, members, products, transactions
- Multi-language support (en-US, vi-VN)
- Multi-currency support
- Real-time dashboard with analytics
- Deep integration with Application and Infrastructure layers

---

## Project Structure

### Root Level Structure

```
WinUI/
├── App.xaml / App.xaml.cs           # Entry point, global resources, DI setup
├── MainWindow.xaml / MainWindow.xaml.cs # Main shell window
├── Package.appxmanifest              # App manifest for packaging
├── WinUI.csproj                      # Project file
├── Assets/                           # Static resources (icons, images, splash)
├── Configurations/                   # Configuration files
├── Converters/                       # Value converters for data binding
├── Helpers/                          # Utility functions and behaviors
├── Properties/                       # Project properties
├── Resources/                        # UI resources (colors, strings, styles)
├── Services/                         # Service implementations
├── Selectors/                        # Data template selectors
├── Themes/                           # Theme resources
├── UIModels/                         # View-specific models
├── ViewModels/                       # MVVM view models
└── Views/                            # XAML views
```

### Detailed Directory Breakdown

#### **1. Views/** - UI Layer
```
Views/
├── Dialogs/
│   ├── LoginDialog.xaml / .xaml.cs
│   ├── RegisterDialog.xaml / .xaml.cs
│   ├── ForgotPasswordDialog.xaml / .xaml.cs
│   ├── ConfigDialog.xaml / .xaml.cs
│   ├── OtpDialog.xaml / .xaml.cs
│   └── Management/
│       ├── ReservationDialog.xaml / .xaml.cs
│       ├── AreaFilterDialog.xaml / .xaml.cs
│       ├── PaymentDialog.xaml / .xaml.cs
│       └── StartSessionDialog.xaml / .xaml.cs
├── Pages/
│   ├── DashboardPage.xaml / .xaml.cs
│   ├── AreaManagementPage.xaml / .xaml.cs
│   ├── GameManagementPage.xaml / .xaml.cs
│   ├── ProductManagementPage.xaml / .xaml.cs
│   ├── MemberManagementPage.xaml / .xaml.cs
│   ├── TransactionManagementPage.xaml / .xaml.cs
│   ├── SettingsPage.xaml / .xaml.cs
│   └── StartingPage.xaml / .xaml.cs
├── UserControls/
│   ├── Dashboard/
│   │   ├── StatCardControl.xaml
│   │   ├── RevenueChartControl.xaml
│   │   ├── QuickStatsControl.xaml
│   │   ├── GoalProgressControl.xaml
│   │   ├── TrendingListControl.xaml
│   │   └── PopularCardControl.xaml
│   ├── AreaManagement/
│   ├── Settings/
│   ├── NavbarControl.xaml / .xaml.cs
│   ├── HeaderControl.xaml / .xaml.cs
│   ├── NotificationControl.xaml / .xaml.cs
│   ├── PaginationControl.xaml / .xaml.cs
│   └── DividerControl.xaml / .xaml.cs
└── Windows/
    └── MainWindow.xaml / .xaml.cs

```

**View Responsibilities**:
- Render UI using XAML
- Bind to ViewModels via `DataContext`
- Handle UI events and route to Commands
- Manage visual states and animations
- NO business logic in code-behind; only navigation/initialization

#### **2. ViewModels/** - Presentation Logic Layer
```
ViewModels/
├── LocalizedViewModelBase.cs         # Base class with localization support
├── MainViewModel.cs                  # Main window viewmodel
├── Pages/
│   ├── DashboardPageViewModel.cs
│   ├── AreaManagementPageViewModel.cs
│   ├── StartingPageViewModel.cs
│   └── SettingsPageViewModel.cs
├── Dialogs/
│   ├── LoginViewModel.cs
│   ├── RegisterViewModel.cs
│   ├── ConfigViewModel.cs
│   ├── ForgotPasswordViewModel.cs
│   ├── OtpViewModel.cs
│   └── Management/
│       ├── ReservationViewModel.cs
│       ├── AreaFilterViewModel.cs
│       └── PaymentViewModel.cs
├── UserControls/
│   ├── NavbarControlViewModel.cs
│   ├── NotificationControlViewModel.cs
│   ├── HeaderControlViewModel.cs
│   ├── PaginationControlViewModel.cs
│   ├── Dashboard/
│   │   ├── StatCardControlViewModel.cs
│   │   ├── RevenueChartControlViewModel.cs
│   │   ├── QuickStatsControlViewModel.cs
│   │   ├── GoalProgressControlViewModel.cs
│   │   └── TrendingListControlViewModel.cs
│   └── Settings/
│       ├── ShopInformationCardControlViewModel.cs
│       └── GeneralSettingsCardControlViewModel.cs
└── AreaManagement/
    ├── SummarizedAreaCards/
    ├── DetailedAreaCards/
    └── ...
```

**ViewModel Responsibilities**:
- Manage presentation state via ObservableProperty
- Expose commands for user interactions
- Invoke Use Cases from Application layer
- Transform domain data into UI-friendly format
- Handle navigation requests
- Implement IDisposable for cleanup

#### **3. Converters/** - Value Binding Converters
```
Converters/
├── BoolToVisibilityConverter.cs      # bool → Visibility enum
├── BoolToOrangeBrushConverter.cs     # bool → Brush color
├── BoolToGradientBrushConverter.cs   # bool → Gradient effects
├── IconConverter.cs                  # IconKind enum → SVG PathGeometry
├── CurrencyConverter.cs              # Decimal → Currency formatted string
├── ChartValueToHeightConverter.cs    # Number → Height for charts
├── DivideByCountConverter.cs         # width / count (layout calculations)
└── ... (7+ converters total)
```

**Converter Patterns**:
- All implement `IValueConverter` interface
- Stateless, thread-safe utility classes
- Support both Convert and ConvertBack where applicable
- Used in XAML bindings: `{Binding Source, Converter={StaticResource ConverterName}}`

#### **4. Helpers/** - Utility Functions
```
Helpers/
├── PasswordBoxBindingHelper.cs       # Enables secure password binding
├── VisualTreeSearchHelper.cs         # Recursively find UIElements
├── CursorBehavior.cs                 # Custom mouse cursor behavior
├── ReservationDateTimeHelper.cs      # DateTime formatting for reservations
└── Validations/                      # Input validation helpers
```

**Key Helper Details**:

**PasswordBoxBindingHelper**: Implements attached properties to bind PasswordBox to viewmodel
```csharp
// Usage in XAML:
<PasswordBox local:PasswordBoxBindingHelper.BindPassword="True"
             local:PasswordBoxBindingHelper.BoundPassword="{Binding Password, Mode=OneWayToSource}" />
```

#### **5. Services/** - Platform-Specific Implementations
```
Services/
├── WinUINavigationService.cs         # INavigationService implementation
├── WinUILocalizationService.cs       # ILocalizationService implementation
├── WinUIDialogService.cs             # IDialogService implementation
├── WinUIAppInfoService.cs            # IAppInfoService implementation
└── Factories/
    ├── StatCardControlViewModelFactory.cs
    ├── PopularCardControlViewModelFactory.cs
    ├── AreaManagementCardViewModelFactory.cs
    ├── AreaModelFactory.cs
    ├── SummarizedAvailableCardViewModelFactory.cs
    ├── SummarizedReservedCardViewModelFactory.cs
    └── SummarizedRentedCardViewModelFactory.cs
```

**Service Implementation Highlights**:

1. **WinUINavigationService**:
   - Maps navigation requests to Page types
   - Uses DI container to instantiate pages
   - Sets Frame.Content directly (avoids XAML instantiation issues)
   - Controls navbar visibility based on page type

2. **WinUILocalizationService**:
   - Implements language, currency, timezone switching
   - Uses Windows ApplicationModel.Resources.ResourceLoader
   - Maintains CultureInfo for locale-specific formatting
   - Fires events on preference changes for UI updates

3. **WinUIDialogService**:
   - Factory-based dialog creation using service provider
   - Shows ContentDialogs with error/confirmation support
   - Requires XamlRoot initialization for proper display

4. **Factory Pattern**:
   - ViewModel factories inject dependencies consistently
   - Example: `StatCardControlViewModelFactory` creates cards with localization

#### **6. UIModels/** - View-Specific Data Models
```
UIModels/
├── Config.cs                         # Server config for UI
├── IconState.cs                      # Icon rendering state
├── LabelValueRowModel.cs             # Label-value pair display
├── LocalizationOptionModel.cs        # Language selection option
├── MenuItemModel.cs                  # Navigation menu items
├── NavbarItemModel.cs                # Navbar button model
├── PaginationModel.cs                # Pagination state
├── ServiceInSessionModel.cs          # Active service tracking
├── Enums/
│   └── IconKind.cs                   # Icon enumeration (50+ types)
├── Dashboard/
│   ├── PopularCardItemData.cs        # Popular items (games/food/drinks)
│   ├── QuickStatsData.cs
│   └── ...
└── Management/
    ├── AreaModel.cs                  # Area display model
    └── ...
```

**UI Model Purpose**:
- NOT domain entities; ViewModels transform entities into these
- Contain only UI-relevant data
- Support binding and display logic
- Decouple presentation from business logic

#### **7. Selectors/** - Data Template Selection
```
Selectors/
├── DetailedAreaCardTemplateSelector.cs   # Selects template based on area status
└── SummarizedAreaCardTemplateSelector.cs # Selects template for area summaries
```

**Purpose**: Dynamically choose XAML DataTemplate based on item type:
```csharp
// In XAML: <ItemsControl ItemTemplateSelector="{StaticResource AreaCardSelector}" ... />
private DataTemplate? ResolveTemplate(object item)
{
    return item switch
    {
        DetailedAvailableCardViewModel => AvailableTemplate,
        DetailedReservedCardViewModel => ReservedTemplate,
        DetailedRentedCardViewModel => RentedTemplate,
        _ => null
    };
}
```

#### **8. Resources/** - Shared UI Resources
```
Resources/
├── AppColors.cs                      # Centralized color definitions
├── AppResourceLookup.cs              # Safe resource lookup with fallbacks
├── Colors.xaml                       # XAML color definitions
├── FontSize.xaml                     # Font size constants
├── Images.xaml                       # Image resources
├── Strings/
│   ├── en-US/Resources.resw          # English localizations
│   └── vi-VN/Resources.resw          # Vietnamese localizations
├── Styles/
│   ├── CommonStyles.xaml             # All merged styles
│   ├── ButtonStyles.xaml
│   ├── CardStyles.xaml
│   ├── DialogStyles.xaml
│   ├── TextInputStyles.xaml
│   ├── GridViewStyles.xaml
│   ├── CheckBoxStyles.xaml
│   ├── ComboBoxStyles.xaml
│   ├── FlyoutStyles.xaml
│   ├── ScrollBarStyles.xaml
│   └── StateResources/               # State-specific resources
└── Animations/                       # Animation definitions
```

**Resource Organization Model**:

```csharp
// AppColors.cs - Centralized color access
public static class AppColors
{
    public static Color PrimaryOrange => 
        AppResourceLookup.GetColor("PrimaryOrange", ColorHelper.FromArgb(0xFF, 0xFF, 0x6B, 0x35));
    
    public static Color ErrorRed => 
        AppResourceLookup.GetColor("ErrorRed", Color.FromArgb(0xFF, 0xDC, 0x26, 0x26));
}

// In code: var brush = new SolidColorBrush(AppColors.PrimaryOrange);
```

#### **9. Configurations/** - App Settings
```
Configurations/
└── appsettings.json
```

**Sample Configuration**:
```json
{
  "AppSettings": {
    "Language": "en-US",
    "Currency": "VND",
    "Timezone": "+7",
    "DateFormat": "dd/MM/yyyy"
  }
}
```

---

## MVVM Architecture

### Core MVVM Pattern Implementation

The project uses **Community Toolkit MVVM** for streamlined MVVM implementation.

### View-ViewModel Binding

**1. Page Binding Pattern**:

```csharp
// DashboardPage.xaml.cs
public sealed partial class DashboardPage : Page
{
    public DashboardPageViewModel ViewModel { get; }

    public DashboardPage(DashboardPageViewModel viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;  // Direct binding
        InitializeComponent();
        Unloaded += HandleUnloaded;
    }

    private void HandleUnloaded(object sender, RoutedEventArgs e)
    {
        Unloaded -= HandleUnloaded;
        ViewModel.Dispose();  // Cleanup
    }
}
```

**2. XAML Binding Syntax**:

```xaml
<!-- Data binding to viewmodel properties -->
<TextBlock Text="{x:Bind ViewModel.Title, Mode=OneWay}" />

<!-- Command binding (ICommand from viewmodel) -->
<Button Command="{x:Bind ViewModel.LoginCommand}">Login</Button>

<!-- Two-way binding with validation -->
<TextBox Text="{x:Bind ViewModel.Email, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />

<!-- Binding with converter -->
<TextBlock Visibility="{x:Bind ViewModel.HasError, Converter={StaticResource BoolToVisibleConverter}}" />
```

### Observable Property Implementation

**MVVM Toolkit Attributes**:

```csharp
public partial class LoginViewModel : LocalizedViewModelBase
{
    // Auto-generates INotifyPropertyChanged with backing field
    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    // Notifies dependent properties and commands on change
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    [NotifyPropertyChangedFor(nameof(CanLoginExecute))]
    public partial bool IsLoggingIn { get; set; }

    // ObservableProperty manages:
    // - INotifyPropertyChanged events
    // - Property change tracking
    // - Command execution state updates
}
```

### Command Pattern

```csharp
// Synchronous command
[RelayCommand]
private void Navigate() 
{
    _navigationService.Navigate(new NavigateToDashboard());
}

// Asynchronous command with execution state
[RelayCommand(CanExecute = nameof(CanLogin))]
private async Task LoginAsync()
{
    IsLoggingIn = true;
    try
    {
        var account = await _loginUseCase.ExecuteAsync(Email, Password);
        LoginSucceededInternal?.Invoke(account);
    }
    finally
    {
        IsLoggingIn = false;
    }
}

private bool CanLogin() => 
    !string.IsNullOrWhiteSpace(Email) && 
    !string.IsNullOrWhiteSpace(Password) && 
    !IsLoggingIn;

// Usage in XAML:
// <Button Command="{x:Bind ViewModel.LoginCommand}" />
```

### Data Binding Patterns

**1. One-Way Binding (ViewModel → View)**:
```xaml
<TextBlock Text="{x:Bind ViewModel.CurrentUser.Name, Mode=OneWay}" />
```

**2. Two-Way Binding (ViewModel ↔ View)**:
```xaml
<TextBox Text="{x:Bind ViewModel.SearchQuery, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
```

**3. One-Time Binding (Load once)**:
```xaml
<TextBlock Text="{x:Bind ViewModel.AppVersion, Mode=OneTime}" />
```

### ViewModel Lifecycle

```
App Constructor
    ↓
DI Setup (Host.CreateDefaultBuilder)
    ↓
Services Registered
    ↓
OnLaunched() Called
    ↓
Configuration Loaded
    ↓
MainWindow Created
    ↓
Page Created (with DI ViewModel)
    ↓
DataContext = ViewModel
    ↓
View Renders (Binding activated)
    ↓
User Interactions
    ↓
Commands Execute
    ↓
ObservableProperty Change Fires Event
    ↓
View Updates Automatically
    ↓
Page Navigated Away
    ↓
Unloaded Event
    ↓
ViewModel.Dispose() Called (cleanup resources)
```

---

## Dependency Injection

### DI Container Setup

The project uses **Microsoft.Extensions.DependencyInjection** and **Microsoft.Extensions.Hosting**.

### Registration Structure

Located in `App.xaml.cs` constructor:

```csharp
Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.SetBasePath(contentRoot);
        config.AddJsonFile("Configurations\\appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        // All services registered here
    })
    .Build();
```

### Service Lifetime Patterns

**1. Singleton Services** (Single instance for app lifetime):
```csharp
services.AddSingleton<ILocalizationService>(sp =>
    new WinUILocalizationService(
        sp.GetRequiredService<Converters.CurrencyConverter>(),
        defaultPreferences.Language,
        defaultPreferences.Currency,
        defaultPreferences.TimeZone));

services.AddSingleton<INavigationService, WinUINavigationService>();
services.AddSingleton<MainViewModel>();
services.AddSingleton<ViewModels.UserControls.NavbarControlViewModel>();
```

**2. Transient Services** (New instance each request):
```csharp
services.AddTransient<ViewModels.Dialogs.LoginViewModel>();
services.AddTransient<ViewModels.Pages.DashboardPageViewModel>();
services.AddTransient<Views.Pages.DashboardPage>();
```

**3. Repository Pattern**:
```csharp
services.AddSingleton<IRepository<Account>, MockAccountRepository>();
services.AddSingleton<IRepository<BoardGame>, MockRepository<BoardGame>>();
services.AddSingleton<IRepository<Product>, MockRepository<Product>>();
services.AddSingleton<IRepository<Member>>(_ => CreateMockMemberRepository());
services.AddSingleton<IRepository<Membership>, MockRepository<Membership>>();
```

### Factory Registration (Advanced Pattern)

```csharp
// Function factory for creating dialog instances on-demand
services.AddSingleton<Func<string, object?, ContentDialog?>>(provider => (dialogKey, parameter) =>
{
    return dialogKey switch
    {
        "Login" => new LoginDialog(provider.GetRequiredService<LoginViewModel>()),
        "Register" => new RegisterDialog(provider.GetRequiredService<RegisterViewModel>()),
        "Otp" => CreateOtpDialog(provider, parameter),
        "Reservation" => CreateReservationDialog(provider, parameter),
        // ...
        _ => null
    };
});

services.AddSingleton<IDialogService, WinUIDialogService>();
```

### Use Case Registration

```csharp
services.AddTransient<Application.UseCases.Auth.LoginUserUseCase>();
services.AddTransient<Application.UseCases.Auth.RegisterUserUseCase>();
services.AddTransient<Application.UseCases.Pagination.BuildPaginationStateUseCase>();
```

### Navigation Map Registration

Post-DI setup, navigation routes are registered:

```csharp
NavigationMap.Register<Application.Navigation.Requests.NavigateToStarting, Views.Pages.StartingPage>();
NavigationMap.Register<Application.Navigation.Requests.NavigateToDashboard, Views.Pages.DashboardPage>();
NavigationMap.Register<Application.Navigation.Requests.NavigateToAreaManagement, Views.Pages.AreaManagementPage>();
// ... more routes
```

### Service Access

**From ViewModels** (Constructor Injection):
```csharp
public LoginViewModel(
    ILocalizationService localizationService,
    IDialogService dialogService,
    LoginUserUseCase loginUseCase,
    INavigationService navigationService,
    INotificationService notificationService,
    MainViewModel mainViewModel)
{
    ArgumentNullException.ThrowIfNull(localizationService);
    // Dependencies are now available
}
```

**From Application** (Service Locator Pattern):
```csharp
var navigationService = App.Host?.Services.GetRequiredService<INavigationService>();
var localizationService = App.Host?.Services.GetRequiredService<ILocalizationService>();
```

---

## Clean Architecture

### Layered Architecture

The WinUI project is part of a larger **Clean Architecture** solution:

```
Domain (Entities, Enums, Business Rules)
    ↑
Application (Use Cases, Interfaces, DTOs)
    ↑
Infrastructure (Repositories, External Services)
    ↑
WinUI (Presentation - This Layer)
```

### Layer Separation

**1. Domain Layer** (Independent):
- Pure business entities
- Enums and value objects
- No external dependencies
- Example: `Account`, `BoardGame`, `PlayArea`

**2. Application Layer** (Business Logic):
- Use Cases (orchestrate domain and repositories)
- Service interfaces `ILocalizationService`, `INavigationService`
- DTOs and contracts
- Example: `LoginUserUseCase`, `BuildPaginationStateUseCase`

**3. Infrastructure Layer** (Data & External Services):
- Repository implementations
- Service implementations (notification, auth, etc.)
- Mock data providers for testing
- Example: `MockAccountRepository`, `DateTimeService`

**4. WinUI Layer** (Presentation):
- Views (XAML UI)
- ViewModels (presentation logic)
- Platform-specific service implementations
- Converters, helpers, dialogs
- **NO business logic** - only calls Application layer

### Abstraction Patterns

**1. Dependency Inversion**:
```csharp
// Bad (Direct dependency):
public LoginViewModel()
{
    _navigationService = new WinUINavigationService();
}

// Good (Abstraction):
public LoginViewModel(INavigationService navigationService)
{
    _navigationService = navigationService ?? throw new ArgumentNullException(...);
}
```

**2. Repository Pattern**:
```csharp
// Abstraction in Application layer:
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    // ...
}

// Used in Application layer Use Cases
// Implemented in Infrastructure layer (Mock or Real)
// WinUI never directly accesses repositories
```

**3. Service Interfaces**:
All services are injected via interfaces:
- `ILocalizationService` (implemented by `WinUILocalizationService`)
- `INavigationService` (implemented by `WinUINavigationService`)
- `IDialogService` (implemented by `WinUIDialogService`)
- `INotificationService` (implemented by `ToastNotificationService`)

### Communication Between Layers

**Example: Login Flow**

```
User Input (View)
    ↓
LoginCommand Executes (ViewModel)
    ↓
Calls LoginUserUseCase (Application)
    ↓
UseCase queries IRepository<Account> (Infrastructure)
    ↓
Repository returns Domain.Entities.Account
    ↓
UseCase returns account to ViewModel
    ↓
ViewModel updates ObservableProperty
    ↓
View automatically updates via binding
    ↓
Navigation via INavigationService (to next page)
```

---

## Key Components Analysis

### 1. App.xaml.cs - Entry Point & DI Bootstrap

**File Structure**:

```csharp
public partial class App : Microsoft.UI.Xaml.Application
{
    public static IHost? Host { get; private set; }
    private Window? _window;

    // Constructor: DI Setup + Startup Logic
    public App()
    {
        // 1. Configure global exception handler
        UnhandledException += App_UnhandledException;
        
        // 2. Initialize XAML (loads App.xaml resources)
        InitializeComponent();

        // 3. Setup DI container
        Host = CreateAndConfigureHost();
    }

    // Lifecycle: Fired after constructor
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        // 1. Load persisted configuration
        // 2. Apply user preferences
        // 3. Create main window
        // 4. Initialize dialog service
        // 5. Navigate to starting page
    }
}
```

**Key Responsibilities**:
- Create and configure IHost (DI container)
- Register all services (Singletons, Transients, Factories)
- Load configuration from `appsettings.json`
- Setup localization preferences
- Handle unhandled exceptions with logging
- Bootstrap main window

**Exception Handling**:
```csharp
private static void App_UnhandledException(...)
{
    LogStartupException($"UnhandledException: {e.Message}", e.Exception);
}

private static void LogStartupException(string context, Exception? exception)
{
    // Writes to: %LocalAppData%/PlayPointPOS/Logs/startup.log
    // Catches exceptions during exception logging to avoid masking
}
```

### 2. MainWindow.xaml & MainWindow.xaml.cs - Shell Window

**Responsibilities**:
- Container for entire application UI
- Hosts main navigation Frame
- Manages title bar customization
- Displays header, navbar, notifications
- Sets up visual effects (tile pattern background)

**Structure**:
```xaml
<Window>
    <Grid RowDefinitions="32, Auto, Auto, *">
        <!-- Row 0: Title bar drag region -->
        <Grid x:Name="TitleBarDragRegion" Grid.Row="0" />
        
        <!-- Row 1: Header (today's revenue, stats) -->
        <controls:HeaderControl Grid.Row="1" />
        
        <!-- Row 2: Navbar (navigation buttons) -->
        <controls:NavbarControl Grid.Row="2" />
        
        <!-- Row 3: Main content frame (pages) -->
        <Frame x:Name="MainFrame" Grid.Row="3" />
    </Grid>
</Window>
```

**Code-Behind**:
```csharp
public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }
    public NavbarControlViewModel NavbarViewModel { get; }

    public MainWindow(
        INavigationService nav,
        MainViewModel viewModel,
        NavbarControlViewModel navbarViewModel, 
        NotificationControlViewModel notificationViewModel,
        ILocalizationService localizationService)
    {
        // 1. Store viewmodels
        // 2. Set DataContext for binding
        // 3. Initialize XAML
        // 4. Configure title bar
        // 5. Setup navigation frame
        // 6. Create visual effects (tile pattern)
        // 7. Navigate to starting page
    }

    private void SetupTilePattern()
    {
        // Loads pattern.png as composited background brush
        // Uses Win2D for visual effects
    }
}
```

### 3. LocalizedViewModelBase - Base Class for Localization

**Purpose**: Common base for ViewModels that need localization support

```csharp
public abstract class LocalizedViewModelBase : ObservableObject, IDisposable
{
    protected LocalizedViewModelBase(ILocalizationService localizationService)
    {
        LocalizationService = localizationService;
        // Subscribe to language/currency/timezone changes
        localizationService.LanguageChanged += HandleLanguageChanged;
        localizationService.CurrencyChanged += HandleCurrencyChanged;
        localizationService.TimeZoneChanged += HandleTimeZoneChanged;
    }

    protected ILocalizationService LocalizationService { get; }

    protected abstract void RefreshLocalizedText();

    // Implements IDisposable to clean up event subscriptions
    public void Dispose()
    {
        LocalizationService.LanguageChanged -= HandleLanguageChanged;
        // ... unsubscribe from other events
    }

    private void HandleLanguageChanged()
    {
        RefreshLocalizedText();  // Called whenever language changes
    }
}
```

**Derived ViewModels Override `RefreshLocalizedText()`**:
```csharp
public sealed class StatCardControlViewModel : LocalizedViewModelBase
{
    protected override void RefreshLocalizedText()
    {
        Title = LocalizationService.GetString($"{_resourcePrefix}Title");
        TrendLabel = LocalizationService.GetString($"{_resourcePrefix}TrendLabel");
        ComparisonLabel = LocalizationService.GetString($"{_resourcePrefix}ComparisonLabel");
        ValueText = _valueTextFactory(LocalizationService);  // Custom formatting
    }
}
```

### 4. Navigation System

**Component: WinUINavigationService**

Implements `INavigationService` from Application layer:

```csharp
public class WinUINavigationService : INavigationService
{
    private Frame? _frame;
    private readonly IServiceProvider _serviceProvider;

    public void SetFrame(object frame)
    {
        if (frame is Frame f)
            _frame = f;
    }

    public void Navigate(INavigationRequest request)
    {
        // 1. Map request type to Page type via NavigationMap
        if (!NavigationMap.Map.TryGetValue(requestType, out var pageType))
            throw new InvalidOperationException(...);

        // 2. Create page instance via DI (ensures dependencies injected)
        var pageInstance = (Page?)_serviceProvider.GetService(pageType);

        // 3. Set as Frame content (not Navigate() to avoid XAML issues)
        _frame.Content = pageInstance;

        // 4. Update UI state
        _mainViewModel.IsNavigationVisible = pageInstance is not StartingPage;
        _navbarViewModel.SelectNavigationItem(requestType);
    }
}
```

**Navigation Request Types** (from Application layer):
```csharp
public record NavigateToStarting : INavigationRequest;
public record NavigateToDashboard : INavigationRequest;
public record NavigateToAreaManagement : INavigationRequest;
// ... more
```

**NavigationMap Registration**:
```csharp
NavigationMap.Register<NavigateToStarting, Views.Pages.StartingPage>();
NavigationMap.Register<NavigateToDashboard, Views.Pages.DashboardPage>();
// Maps request type → Page type
```

### 5. Dialog Service

**Pattern: Factory-Based Dialog Creation**

```csharp
// Factory registered in DI:
services.AddSingleton<Func<string, object?, ContentDialog?>>(provider => (dialogKey, parameter) =>
{
    return dialogKey switch
    {
        "Login" => new LoginDialog(provider.GetRequiredService<LoginViewModel>()),
        "Register" => new RegisterDialog(provider.GetRequiredService<RegisterViewModel>()),
        "Otp" => CreateOtpDialog(provider, parameter),
        _ => null
    };
});

// Used by IDialogService:
public async Task ShowDialogAsync(string dialogKey, object? parameter)
{
    var dialog = _dialogFactory(dialogKey, parameter);
    dialog.XamlRoot = _rootElement.XamlRoot;
    await dialog.ShowAsync();
}
```

---

## Design Patterns

### 1. MVVM Pattern
- **View**: Data binding target, no logic
- **ViewModel**: Commands, observable properties, presentation state
- **Model**: Business entities and use cases

### 2. Repository Pattern
- Abstract data access via `IRepository<T>`
- Mock implementations for development
- Decouples business logic from data source

### 3. Factory Pattern
- ViewModel factories (StatCardControlViewModelFactory, etc.)
- DialogFactory for dialog creation
- Page factories via DI

### 4. Dependency Injection
- Constructor-based injection
- Composition root (App.cs)
- Service locator pattern (App.Host)

### 5. Command Pattern
- RelayCommand for synchronous operations
- AsyncRelayCommand for async operations
- CanExecute pattern for UI state

### 6. Observer Pattern
- ObservableProperty (property changed notifications)
- Event subscriptions (LanguageChanged, CurrencyChanged)
- Binding engine observes and updates UI

### 7. Strategy Pattern
- Data template selectors (DetailedAreaCardTemplateSelector)
- Different UI templates based on data type

### 8. Converter Pattern
- IValueConverter implementations
- Transform data for UI display
- Used in XAML bindings

### 9. Adapter Pattern
- Platform-specific service implementations
- Example: WinUILocalizationService adapts to Windows APIs

### 10. Service Locator Pattern
- Used sparingly: `App.Host?.Services.GetRequiredService<T>()`
- Preferred: Constructor injection

---

## Clean Code Practices

### 1. Naming Conventions

**Observable Properties**:
```csharp
[ObservableProperty]
public partial string Email { get; set; }  // PascalCase, public

[ObservableProperty]
private partial bool _isLoading;  // Private fields NOT prefixed with underscore
```

**Private Fields**:
```csharp
private readonly IDialogService _dialogService;  // underscore prefix
private bool _isDisposed;
```

**Constants**:
```csharp
private const string AppDataFolderName = "PlayPointPOS";  // UPPER_CASE or PascalCase
private const int PreferredAreaCardsPerRow = 3;
private const double MinimumSingleColumnWidth = 240;
```

**Commands**:
```csharp
[RelayCommand]
private async Task LoginAsync() { }  // Async suffix for Task-returning methods
// Public property: LoginCommand (auto-generated)
```

### 2. Code Organization in ViewModels

```csharp
public partial class ExampleViewModel : LocalizedViewModelBase
{
    // 1. Private readonly fields (dependencies)
    private readonly IService _service;

    // 2. Observable properties
    [ObservableProperty]
    public partial string Title { get; set; }

    // 3. Computed properties
    public bool HasItems => Items.Count > 0;

    // 4. Constructors (dependency injection)
    public ExampleViewModel(IService service, ILocalizationService localization)
        : base(localization)
    {
        _service = service;
    }

    // 5. Command methods
    [RelayCommand]
    private async Task LoadDataAsync() { }

    // 6. Event handlers
    private void HandleDataLoaded() { }

    // 7. Override methods (RefreshLocalizedText, etc.)
    protected override void RefreshLocalizedText() { }

    // 8. Cleanup
    public void Dispose() { }
}
```

### 3. Property Organization

**ObservableProperty with Dependencies**:
```csharp
[ObservableProperty]
[NotifyCanExecuteChangedFor(nameof(SubmitCommand))]  // Invalidate command
[NotifyPropertyChangedFor(nameof(CanSubmit))]        // Notify computed property
public partial bool IsSubmitting { get; set; }

// Triggers:
// 1. PropertyChanged event
// 2. SubmitCommand.CanExecuteChanged event
// 3. PropertyChanged for CanSubmit computed property
```

### 4. Error Handling

**Application-Level Exception Handling**:
```csharp
// App.xaml.cs
private static void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    LogStartupException($"UnhandledException: {e.Message}", e.Exception);
}

private static void LogStartupException(string context, Exception? exception)
{
    try
    {
        string logPath = Path.Combine(logDirectory, "startup.log");
        string details = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {context}{Environment.NewLine}{exception}...";
        File.AppendAllText(logPath, details);
    }
    catch
    {
        // Avoid masking original exception when logging fails
    }
}
```

**Command Exception Handling**:
```csharp
[RelayCommand(CanExecute = nameof(CanLogin))]
private async Task LoginAsync()
{
    IsLoggingIn = true;
    try
    {
        var account = await _loginUseCase.ExecuteAsync(Email, Password);
        LoginSucceededInternal?.Invoke(account);
    }
    catch (Exception ex)
    {
        HasError = true;
        ErrorMessage = ex.Message;
        await _dialogService.ShowErrorAsync(ex.Message);
    }
    finally
    {
        IsLoggingIn = false;
    }
}
```

**Validation Patterns**:
```csharp
private bool TryParseOptionalDecimal(string? text, out decimal? value)
{
    value = null;
    string trimmed = text?.Trim() ?? string.Empty;

    if (string.IsNullOrWhiteSpace(trimmed))
        return true;  // Empty is valid

    bool success = decimal.TryParse(trimmed, NumberStyles.Number, Culture, out var parsed)
                || decimal.TryParse(trimmed, NumberStyles.Number, InvariantCulture, out parsed);

    value = success ? Math.Max(0m, parsed) : null;
    return success;
}
```

### 5. Async/Await Usage

**Commands with Async Operations**:
```csharp
[RelayCommand(CanExecute = nameof(CanSubmit))]
private async Task SubmitAsync()
{
    IsSubmitting = true;
    try
    {
        await _useCase.ExecuteAsync(Data);
        await _dialogService.ShowConfirmationAsync(...);
    }
    catch (Exception ex)
    {
        await _dialogService.ShowErrorAsync(ex.Message);
    }
    finally
    {
        IsSubmitting = false;
    }
}

// CanExecute is evaluated before method runs
private bool CanSubmit() => !IsSubmitting && HasData;
```

**Loading States**:
```csharp
[ObservableProperty]
public partial bool IsLoading { get; set; }

[RelayCommand]
private async Task RefreshAsync()
{
    IsLoading = true;
    try
    {
        Items = new(await _repository.GetAllAsync());
    }
    finally
    {
        IsLoading = false;
    }
}

// In XAML: Show spinner when IsLoading = true
```

### 6. Resource Cleanup

**IDisposable Implementation**:
```csharp
public sealed class ExampleViewModel : LocalizedViewModelBase
{
    private bool _isDisposed;

    public void Dispose()
    {
        if (_isDisposed)
            return;

        base.Dispose();  // Base cleanup
        _dialogService?.Dispose();
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
}

// In View code-behind:
private void HandleUnloaded(object sender, RoutedEventArgs e)
{
    Unloaded -= HandleUnloaded;
    ViewModel.Dispose();
}
```

### 7. Property Validation

```csharp
[ObservableProperty]
[NotifyCanExecuteChangedFor(nameof(LoginCommand))]
public partial string Email { get; set; } = string.Empty;

private bool CanLogin() =>
    !string.IsNullOrWhiteSpace(Email) &&
    !string.IsNullOrWhiteSpace(Password) &&
    !IsLoggingIn;

// Visual feedback in XAML:
// <Button ... IsEnabled="{x:Bind ViewModel.CanLoginExecute, Mode=OneWay}" />
```

---

## Resources & Styling

### 1. Color System

**Centralized in AppColors.cs**:
```csharp
public static class AppColors
{
    // Primary Brand
    public static Color PrimaryOrange => AppResourceLookup.GetColor("PrimaryOrange", #FFFF6B35);
    
    // Neutral
    public static Color White => AppResourceLookup.GetColor("White", #FFFFFFFF);
    public static Color Black => AppResourceLookup.GetColor("Black", #FF1F1F1F);
    public static Color LightGray => AppResourceLookup.GetColor("LightGray", #FFD8D8D8);
    
    // Status
    public static Color SuccessGreen => AppResourceLookup.GetColor("SuccessGreen", #FF10B981);
    public static Color ErrorRed => AppResourceLookup.GetColor("ErrorRed", #FFDC2626);
    public static Color WarningAmber => AppResourceLookup.GetColor("WarningAmber", #FFF59E0B);
    
    // Dashboard
    public static Color DashboardTrendPositive => SuccessGreen;
    public static Color DashboardTrendNegative => ErrorRed;
}
```

**Usage in Code**:
```csharp
var brush = new SolidColorBrush(AppColors.PrimaryOrange);
var errorBrush = new SolidColorBrush(AppColors.ErrorRed);
```

### 2. Localized Strings

**Resource Structure**:
```
Resources/Strings/
├── en-US/Resources.resw (900+ entries)
└── vi-VN/Resources.resw (900+ entries)
```

**Sample Entries**:
```
AppDisplayNameText: "PlayPoint POS"
LoginTitleText: "Login"
EmailLabelText: "Email"
PasswordLabelText: "Password"
DialogErrorTitleText: "Error"
```

**Access in Code**:
```csharp
string title = localizationService.GetString("LoginTitleText");
// Returns: "Login" (en-US) or "Đăng nhập" (vi-VN)
```

**Access in XAML**:
```xaml
<TextBlock Text="{x:Bind ViewModel.LoginTitle}" />
<!-- ViewModel retrieves via localization service -->
```

### 3. Style System

**File Structure**:
```
Styles/
├── CommonStyles.xaml (merges all below)
├── ButtonStyles.xaml
├── CardStyles.xaml
├── DialogStyles.xaml
├── TextInputStyles.xaml
├── GridViewStyles.xaml
├── CheckBoxStyles.xaml
├── ComboBoxStyles.xaml
└── FlyoutStyles.xaml
```

**Style Definitions**:
```xaml
<!-- ButtonStyles.xaml -->
<Style x:Key="PrimaryButtonStyle" TargetType="Button">
    <Setter Property="Background" Value="{ThemeResource PrimaryOrangeBrush}" />
    <Setter Property="Foreground" Value="White" />
    <Setter Property="Padding" Value="12,8" />
    <Setter Property="CornerRadius" Value="4" />
    <!-- ... -->
</Style>

<!-- Usage -->
<Button Style="{StaticResource PrimaryButtonStyle}">Submit</Button>
```

**Theme Dictionary**:
```xaml
<Application.Resources>
    <ResourceDictionary.MergedDictionaries>
        <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
        <ResourceDictionary Source="ms-appx:///Resources/FontSize.xaml" />
        <ResourceDictionary Source="ms-appx:///Resources/Colors.xaml" />
        <ResourceDictionary Source="ms-appx:///Resources/Images.xaml" />
        <ResourceDictionary Source="ms-appx:///Resources/Styles/CommonStyles.xaml" />
    </ResourceDictionary.MergedDictionaries>

    <ResourceDictionary.ThemeDictionaries>
        <ResourceDictionary x:Key="Default">
            <!-- Light theme colors -->
        </ResourceDictionary>
    </ResourceDictionary.ThemeDictionaries>
</Application.Resources>
```

---

## Configuration & Environment

### Configuration File

**Location**: `Configurations/appsettings.json`

```json
{
  "AppSettings": {
    "Language": "en-US",
    "Currency": "VND",
    "Timezone": "+7",
    "DateFormat": "dd/MM/yyyy"
  }
}
```

### Configuration Loading

**In App.xaml.cs**:
```csharp
.ConfigureAppConfiguration((context, config) =>
{
    config.SetBasePath(contentRoot);
    config.AddJsonFile("Configurations\\appsettings.json", optional: true, reloadOnChange: true);
})
```

### Configuration Access

**In Services**:
```csharp
var localizationService = new WinUILocalizationService(
    currencyConverter,
    context.Configuration["AppSettings:Language"] ?? "en-US",
    context.Configuration["AppSettings:Currency"] ?? "VND",
    context.Configuration["AppSettings:Timezone"] ?? "+7"
);
```

### Persistent Configuration

**User Preferences Storage**:
- Location: `%LocalAppData%/PlayPointPOS/Configurations/config.json`
- Loaded via `IConfigurationService` during `OnLaunched()`
- Preferences: Language, Currency, TimeZone, DateFormat

---

## Navigation System

### Navigation Flow

```
User clicks navbar item
    ↓
NavbarItemModel.RequestType identified
    ↓
NavbarControlViewModel.Navigate(NavbarItemModel)
    ↓
Creates navigation request (e.g., NavigateToDashboard)
    ↓
Calls INavigationService.Navigate(request)
    ↓
WinUINavigationService looks up Page type in NavigationMap
    ↓
Creates page instance via DI (injects ViewModel)
    ↓
Sets Frame.Content = page instance
    ↓
Page.xaml_cs sets DataContext = ViewModel
    ↓
Bindings activate
    ↓
View displays
```

### Navbar Implementation

**NavbarControlViewModel**:
```csharp
public partial class NavbarControlViewModel : LocalizedViewModelBase
{
    public ObservableCollection<NavbarItemModel> NavigationItems { get; } = [];

    public IRelayCommand<NavbarItemModel> NavigateCommand { get; }

    public NavbarControlViewModel(INavigationService navigationService, ...)
    {
        NavigateCommand = new RelayCommand<NavbarItemModel>(Navigate);
        InitializeNavigationItems();
    }

    private void Navigate(NavbarItemModel item)
    {
        var request = (INavigationRequest?)Activator.CreateInstance(item.RequestType);
        if (request != null)
            _navigationService.Navigate(request);
    }

    public void SelectNavigationItem(Type? requestType)
    {
        foreach (var item in NavigationItems)
            item.IsSelected = requestType == item.RequestType;
    }
}
```

**NavbarItemModel**:
```csharp
public class NavbarItemModel
{
    public string Label { get; set; }
    public string LabelResourceKey { get; set; }
    public Type RequestType { get; set; }  // e.g., NavigateToDashboard
    public IconState IconState { get; set; }
    public bool IsSelected { get; set; }
}
```

---

## State Management

### ViewModel-Based State

**Single-Page ViewModel State**:
```csharp
[ObservableProperty]
public partial string SearchQuery { get; set; }

[ObservableProperty]
public partial ObservableCollection<AreaModel> Items { get; set; } = [];

[ObservableProperty]
public partial bool IsLoading { get; set; }

[ObservableProperty]
public partial string ErrorMessage { get; set; }
```

### Shared Application State

**MainViewModel** (Global State):
```csharp
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    public partial bool IsNavigationVisible { get; set; }

    [ObservableProperty]
    public partial string TodayRevenue { get; set; }

    [ObservableProperty]
    public partial string ActiveAreas { get; set; }
}
```

### Domain Model State

State is reflected through Domain entities from repositories:
```csharp
public class Account
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public AccountStatus Status { get; set; }
}
```

### Localization State

**Managed by ILocalizationService Singleton**:
```csharp
public interface ILocalizationService
{
    string Language { get; }
    string Currency { get; }
    string TimeZone { get; }
    CultureInfo Culture { get; }

    event Action? LanguageChanged;
    event Action? CurrencyChanged;
    event Action? TimeZoneChanged;

    void ChangeLanguage(string lang);
    void ChangeCurrency(string currency);
    void ChangeTimeZone(string timeZone);
}
```

### Notification State

**Managed by NotificationControlViewModel**:
- Observes notification events
- Displays toast/snackbar notifications
- Updates unread count

### Dialog State Management

Dialogs receive parameter and return result:
```csharp
public async Task<bool> ShowConfirmationAsync(string titleKey, string messageKey)
{
    var dialog = new ConfirmationDialog(...);
    var result = await dialog.ShowAsync();  // ContentDialogResult.Primary or Secondary
    return result == ContentDialogResult.Primary;
}
```

---

## Summary of Architecture

### Key Principles

1. **Separation of Concerns**
   - Views have NO logic
   - ViewModels have presentation logic
   - Services handle cross-cutting concerns
   - Application layer has business logic

2. **Dependency Injection**
   - All dependencies injected via constructors
   - Single composition root (App.cs)
   - Loose coupling between components

3. **MVVM Pattern**
   - Observable properties for state
   - Commands for user interactions
   - Data binding for automatic UI updates

4. **Asynchronous Operations**
   - RelayCommand for async operations
   - Proper state management during async work
   - Exception handling in try-catch-finally

5. **Localization & Configuration**
   - Centralized string resources
   - Multi-language support
   - Persistent user preferences

6. **Clean Code**
   - Consistent naming conventions
   - Proper resource cleanup (IDisposable)
   - Clear code organization
   - Comprehensive error handling

### Strengths

✅ **Strong separation of layers** - Business logic isolated from UI
✅ **Testable architecture** - Services abstracted via interfaces
✅ **Maintainable code** - Clear structure and patterns
✅ **Scalable design** - Easy to add new pages/services
✅ **Multi-language support** - Built-in localization
✅ **DI container** - Loosely coupled components
✅ **Observable properties** - Automatic UI synchronization
✅ **Comprehensive error handling** - Graceful degradation

### Areas for Enhancement

⚠️ **State Management** - Complex features might benefit from Flux/Redux pattern
⚠️ **Caching strategy** - Current approach doesn't cache API responses
⚠️ **Async state tracking** - Could use more sophisticated loading indicators
⚠️ **Validation framework** - Validation logic could be centralized
⚠️ **API client** - Mock repositories should eventually use real GraphQL client

---

## Conclusion

The WinUI project exemplifies a **well-structured, clean architecture** with:
- Clear separation between presentation, business logic, and data layers
- MVVM pattern with strong tooling support (MVVM Toolkit)
- Comprehensive use of design patterns (Factory, Repository, Observer, etc.)
- Professional code organization and error handling
- Support for multi-language, multi-currency, and multi-timezone scenarios

The architecture is production-ready and provides a solid foundation for desktop application development with WinUI 3.
