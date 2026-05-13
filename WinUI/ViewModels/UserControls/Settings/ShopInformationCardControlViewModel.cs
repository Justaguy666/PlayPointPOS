using System;
using System.Threading.Tasks;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinUI.Helpers.Validations;
using WinUI.ViewModels;
using WinUI.ViewModels.Dialogs;

namespace WinUI.ViewModels.UserControls.Settings;

public partial class ShopInformationCardControlViewModel : LocalizedViewModelBase
{
    private readonly IConfigurationService _configurationService;
    private readonly IManagementApiService _managementApiService;
    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;
    private string _appliedShopName = string.Empty;
    private string _appliedAddress = string.Empty;
    private string _appliedEmail = string.Empty;
    private string _appliedPhone = string.Empty;
    private bool _isApplying;

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ShopNameLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ShopNamePlaceholder { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    public partial string ShopName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AddressLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AddressPlaceholder { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    public partial string Address { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmailLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmailPlaceholder { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PhoneLabel { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PhonePlaceholder { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    public partial string Phone { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ApplyButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmailErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasEmailError { get; set; }

    [ObservableProperty]
    public partial string PhoneErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool HasPhoneError { get; set; }

    public ShopInformationCardControlViewModel(
        ILocalizationService localizationService,
        IConfigurationService configurationService,
        IManagementApiService managementApiService,
        IDialogService dialogService,
        INotificationService notificationService)
        : base(localizationService)
    {
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        _managementApiService = managementApiService ?? throw new ArgumentNullException(nameof(managementApiService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        LoadShopProfileFromConfig();
        RefreshLocalizedText();
        StoreAppliedValues();
        _ = LoadShopProfileFromBackendAsync();
    }

    protected override void RefreshLocalizedText()
    {
        Title = LocalizationService.GetString("SettingsShopInformationTitle");
        ShopNameLabel = LocalizationService.GetString("SettingsShopInformationShopNameLabel");
        ShopNamePlaceholder = LocalizationService.GetString("SettingsShopInformationShopNamePlaceholder");
        AddressLabel = LocalizationService.GetString("SettingsShopInformationAddressLabel");
        AddressPlaceholder = LocalizationService.GetString("SettingsShopInformationAddressPlaceholder");
        EmailLabel = LocalizationService.GetString("SettingsShopInformationEmailLabel");
        EmailPlaceholder = LocalizationService.GetString("SettingsShopInformationEmailPlaceholder");
        PhoneLabel = LocalizationService.GetString("SettingsShopInformationPhoneLabel");
        PhonePlaceholder = LocalizationService.GetString("SettingsShopInformationPhonePlaceholder");
        ApplyButtonText = LocalizationService.GetString("SettingsPageApplyButton");
        ValidateEmail();
        ValidatePhone();
        ApplyCommand.NotifyCanExecuteChanged();
    }

    partial void OnEmailChanged(string value)
    {
        ValidateEmail();
        ApplyCommand.NotifyCanExecuteChanged();
    }

    partial void OnPhoneChanged(string value)
    {
        ValidatePhone();
        ApplyCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanApply))]
    private async Task ApplyAsync()
    {
        if (_isApplying)
        {
            return;
        }

        _isApplying = true;
        ApplyCommand.NotifyCanExecuteChanged();

        try
        {
            if (HasEmailChanged())
            {
                await _dialogService.ShowDialogAsync(
                    "Otp",
                    new OtpDialogRequest
                    {
                        Mode = OtpDialogMode.VerifyEmailChange,
                        PendingEmail = Normalize(Email),
                        OnVerifiedAsync = ApplyChangesAsync
                    });
                return;
            }

            await ApplyChangesAsync();
        }
        finally
        {
            _isApplying = false;
            ApplyCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanApply()
        => !_isApplying
        && !string.IsNullOrWhiteSpace(ShopName)
        && !string.IsNullOrWhiteSpace(Address)
        && !string.IsNullOrWhiteSpace(Email)
        && !string.IsNullOrWhiteSpace(Phone)
        && !HasEmailError
        && !HasPhoneError
        && HasChanges();

    private async Task ApplyChangesAsync()
    {
        string normalizedShopName = Normalize(ShopName);
        string normalizedAddress = Normalize(Address);
        string normalizedEmail = Normalize(Email);
        string normalizedPhone = Normalize(Phone);

        ShopProfile updatedProfile = await _managementApiService.UpdateShopProfileAsync(new ShopProfile
        {
            ShopName = normalizedShopName,
            Address = normalizedAddress,
            Email = normalizedEmail,
            Phone = normalizedPhone,
        });
        await _configurationService.SaveShopProfileAsync(updatedProfile);

        ShopName = Normalize(updatedProfile.ShopName);
        Address = Normalize(updatedProfile.Address);
        Email = Normalize(updatedProfile.Email);
        Phone = Normalize(updatedProfile.Phone);
        StoreAppliedValues();

        await _notificationService.SendAsync(
            LocalizationService.GetString("SettingsShopInformationSavedTitle"),
            string.Format(LocalizationService.Culture, LocalizationService.GetString("SettingsShopInformationSavedMessage"), ShopName),
            NotificationType.Success);

        ApplyCommand.NotifyCanExecuteChanged();
    }

    private bool HasChanges()
        => Normalize(ShopName) != _appliedShopName
        || Normalize(Address) != _appliedAddress
        || Normalize(Email) != _appliedEmail
        || Normalize(Phone) != _appliedPhone;

    private bool HasEmailChanged()
        => Normalize(Email) != _appliedEmail;

    private void StoreAppliedValues()
    {
        ShopName = Normalize(ShopName);
        Address = Normalize(Address);
        Email = Normalize(Email);
        Phone = Normalize(Phone);

        _appliedShopName = ShopName;
        _appliedAddress = Address;
        _appliedEmail = Email;
        _appliedPhone = Phone;
    }

    private static string Normalize(string? value)
        => value?.Trim() ?? string.Empty;

    private void LoadShopProfileFromConfig()
    {
        ShopProfile profile = _configurationService.ShopProfile;
        ShopName = Normalize(profile.ShopName);
        Address = Normalize(profile.Address);
        Email = Normalize(profile.Email);
        Phone = Normalize(profile.Phone);

        if (string.IsNullOrWhiteSpace(Email))
        {
            Email = Normalize(_configurationService.RememberedEmail);
        }
    }

    private async Task LoadShopProfileFromBackendAsync()
    {
        try
        {
            ShopProfile profileFromBackend = await _managementApiService.GetShopProfileAsync();
            if (string.IsNullOrWhiteSpace(profileFromBackend.ShopName)
                && string.IsNullOrWhiteSpace(profileFromBackend.Address)
                && string.IsNullOrWhiteSpace(profileFromBackend.Email)
                && string.IsNullOrWhiteSpace(profileFromBackend.Phone))
            {
                return;
            }

            ShopName = Normalize(profileFromBackend.ShopName);
            Address = Normalize(profileFromBackend.Address);
            Email = Normalize(profileFromBackend.Email);
            Phone = Normalize(profileFromBackend.Phone);

            if (string.IsNullOrWhiteSpace(Email))
            {
                Email = Normalize(_configurationService.RememberedEmail);
            }

            StoreAppliedValues();
            await _configurationService.SaveShopProfileAsync(new ShopProfile
            {
                ShopName = ShopName,
                Address = Address,
                Email = Email,
                Phone = Phone,
            });
        }
        catch
        {
            // Keep local configuration values when backend profile is temporarily unavailable.
        }
        finally
        {
            ApplyCommand.NotifyCanExecuteChanged();
        }
    }

    private void ValidateEmail()
    {
        string normalizedEmail = Normalize(Email);
        bool isInvalid = !string.IsNullOrWhiteSpace(normalizedEmail) && !EmailValidation.IsValid(normalizedEmail);

        HasEmailError = isInvalid;
        EmailErrorMessage = isInvalid
            ? LocalizationService.GetString("SettingsShopInformationInvalidEmailText")
            : string.Empty;
    }

    private void ValidatePhone()
    {
        string normalizedPhone = Normalize(Phone);
        bool isInvalid = !string.IsNullOrWhiteSpace(normalizedPhone) && !PhoneValidation.IsValid(normalizedPhone);

        HasPhoneError = isInvalid;
        PhoneErrorMessage = isInvalid
            ? LocalizationService.GetString("SettingsShopInformationInvalidPhoneText")
            : string.Empty;
    }
}
