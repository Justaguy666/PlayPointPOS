using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Enums;
using WinUI.Services.Factories;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;

namespace WinUI.ViewModels.Dialogs.Management;

public partial class ProductDialogViewModel : UpsertDialogViewModelBase
{
    private const string DefaultImageUri = "ms-appx:///Assets/Mock.png";
    private const string ProductTypeFoodValue = "food";
    private const string ProductTypeDrinkValue = "drink";

    private readonly IDialogService _dialogService;
    private readonly ProductModelFactory _productModelFactory;
    private ProductModel _targetModel = new();
    private ProductModel _initialModel = new();
    private Func<ProductModel, Task>? _onSubmittedAsync;
    private bool _isUpdatingSelectionOptions;
    private event Action? CloseRequestedInternal;

    public event Action? DialogHideRequested;

    public event Action? DialogShowRequested;

    public ProductDialogViewModel(
        ILocalizationService localizationService,
        IDialogService dialogService,
        ProductModelFactory productModelFactory,
        UpsertDialogMode mode)
        : base(localizationService, mode)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _productModelFactory = productModelFactory ?? throw new ArgumentNullException(nameof(productModelFactory));
        ApplyModel(_initialModel);
    }

    [ObservableProperty]
    public partial string NameLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NamePlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProductTypeLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PriceLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PricePlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ImageUriLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ImageUriPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string BrowseImageButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ResetButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial string ProductName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial string? SelectedProductTypeValue { get; set; } = ProductTypeFoodValue;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    public partial string PriceText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    [NotifyPropertyChangedFor(nameof(ImagePreviewSource))]
    public partial string ImageUriText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string ErrorMessage { get; set; } = string.Empty;

    public ObservableCollection<LocalizationOptionModel> ProductTypeOptions { get; } = [];

    protected override string CreateTitleLocKey => "ProductDialogCreateTitleText";

    protected override string EditTitleLocKey => "ProductDialogEditTitleText";

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public string ImagePreviewSource => string.IsNullOrWhiteSpace(ImageUriText)
        ? DefaultImageUri
        : ImageUriText;

    public bool CanSubmit => TryGetParsedFormValues(
        out _,
        out _,
        out _,
        out _);

    public event Action? CloseRequested
    {
        add => CloseRequestedInternal += value;
        remove => CloseRequestedInternal -= value;
    }

    protected override void RefreshLocalizedText()
    {
        base.RefreshLocalizedText();

        NameLabelText = LocalizationService.GetString("ProductDialogNameLabelText");
        NamePlaceholderText = LocalizationService.GetString("ProductDialogNamePlaceholderText");
        ProductTypeLabelText = LocalizationService.GetString("ProductDialogProductTypeLabelText");
        PriceLabelText = LocalizationService.GetString("ProductDialogPriceLabelText");
        PricePlaceholderText = LocalizationService.GetString("ProductDialogPricePlaceholderText");
        ImageUriLabelText = LocalizationService.GetString("ProductDialogImageUriLabelText");
        ImageUriPlaceholderText = LocalizationService.GetString("ProductDialogImageUriPlaceholderText");
        BrowseImageButtonText = LocalizationService.GetString("BrowseImageButtonText");
        ResetButtonText = LocalizationService.GetString("ProductDialogResetButtonText");
        CloseTooltipText = LocalizationService.GetString("CloseTooltipText");
        SubmitButtonText = IsEdit
            ? LocalizationService.GetString("SaveButtonText")
            : LocalizationService.GetString("ProductDialogApplyButtonText");

        RefreshSelectionOptions();
    }

    public void Configure(ProductDialogRequest? request)
    {
        _onSubmittedAsync = request?.OnSubmittedAsync;
        _targetModel = request?.Model ?? CreateDefaultModel();
        _initialModel = _productModelFactory.Clone(_targetModel);
        ErrorMessage = string.Empty;

        RefreshSelectionOptions();
        ApplyModel(_initialModel);
    }

    public override Task SaveAsync() => SubmitAsync();

    [RelayCommand]
    private async Task CloseAsync()
    {
        DialogHideRequested?.Invoke();

        bool isConfirmed = await _dialogService.ShowConfirmationAsync(
            titleKey: "ConfirmCloseTitle",
            messageKey: "ConfirmCloseMessage",
            confirmButtonTextKey: "ConfirmCloseButton",
            cancelButtonTextKey: "CancelButtonText");

        if (isConfirmed)
        {
            CloseRequestedInternal?.Invoke();
            return;
        }

        DialogShowRequested?.Invoke();
    }

    [RelayCommand]
    private void Reset()
    {
        ErrorMessage = string.Empty;
        ApplyModel(_initialModel);
    }

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        if (!TryApplyToTargetModel(out ProductModel model))
        {
            return;
        }

        DialogHideRequested?.Invoke();

        bool isConfirmed = await _dialogService.ShowConfirmationAsync(
            titleKey: IsEdit ? "ConfirmEditProductTitle" : "ConfirmAddProductTitle",
            messageKey: IsEdit ? "ConfirmEditProductMessage" : "ConfirmAddProductMessage",
            confirmButtonTextKey: IsEdit ? "ConfirmEditProductButton" : "ConfirmAddProductButton",
            cancelButtonTextKey: "CancelButtonText");

        if (!isConfirmed)
        {
            DialogShowRequested?.Invoke();
            return;
        }

        ErrorMessage = string.Empty;

        try
        {
            if (_onSubmittedAsync is not null)
            {
                await _onSubmittedAsync(model);
            }

            CloseRequestedInternal?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            DialogShowRequested?.Invoke();
        }
    }

    partial void OnProductNameChanged(string value) => NotifyFormStateChanged();

    partial void OnSelectedProductTypeValueChanged(string? value)
    {
        if (!_isUpdatingSelectionOptions)
        {
            NotifyFormStateChanged();
        }
    }

    partial void OnPriceTextChanged(string value) => NotifyFormStateChanged();

    partial void OnImageUriTextChanged(string value) => NotifyFormStateChanged();

    private void RefreshSelectionOptions()
    {
        _isUpdatingSelectionOptions = true;

        try
        {
            string currentProductTypeValue = SelectedProductTypeValue ?? ProductTypeFoodValue;

            ReplaceOptions(
                ProductTypeOptions,
                [
                    new LocalizationOptionModel { Value = ProductTypeFoodValue, DisplayName = LocalizationService.GetString("ProductTypeFoodText") },
                    new LocalizationOptionModel { Value = ProductTypeDrinkValue, DisplayName = LocalizationService.GetString("ProductTypeDrinkText") },
                ]);

            if (!ProductTypeOptions.Any(option => option.Value == currentProductTypeValue))
            {
                currentProductTypeValue = ProductTypeFoodValue;
            }

            SelectedProductTypeValue = currentProductTypeValue;
        }
        finally
        {
            _isUpdatingSelectionOptions = false;
        }
    }

    private void ApplyModel(ProductModel model)
    {
        ProductName = model.Name;
        SelectedProductTypeValue = MapProductTypeToValue(model.ProductType);
        PriceText = model.Price > 0m
            ? model.Price.ToString("0.##", LocalizationService.Culture)
            : string.Empty;
        ImageUriText = NormalizeEditableImageUri(model.ImageUri);
        NotifyFormStateChanged();
    }

    private void NotifyFormStateChanged()
    {
        if (!string.IsNullOrWhiteSpace(ErrorMessage))
        {
            ErrorMessage = string.Empty;
        }

        SubmitCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanSubmit));
    }

    private ProductModel CreateDefaultModel()
    {
        return new ProductModel
        {
            ProductType = ProductType.Food,
            StockQuantity = 0,
            ImageUri = DefaultImageUri,
        };
    }

    private bool TryApplyToTargetModel(out ProductModel model)
    {
        model = _targetModel;

        if (!TryGetParsedFormValues(
                out string trimmedName,
                out ProductType productType,
                out decimal price,
                out string imageUri))
        {
            return false;
        }

        model.Name = trimmedName;
        model.ProductType = productType;
        model.Price = price;
        model.ImageUri = imageUri;
        model.StockQuantity = 0;

        return true;
    }

    private bool TryGetParsedFormValues(
        out string trimmedName,
        out ProductType productType,
        out decimal price,
        out string imageUri)
    {
        trimmedName = ProductName?.Trim() ?? string.Empty;
        productType = ResolveSelectedProductType();
        imageUri = string.IsNullOrWhiteSpace(ImageUriText) ? DefaultImageUri : ImageUriText.Trim();

        if (string.IsNullOrWhiteSpace(trimmedName) || string.IsNullOrWhiteSpace(SelectedProductTypeValue))
        {
            price = 0m;
            return false;
        }

        if (!TryParseRequiredDecimal(PriceText, out price))
        {
            price = 0m;
            return false;
        }

        return price > 0m;
    }

    private ProductType ResolveSelectedProductType()
    {
        return SelectedProductTypeValue switch
        {
            ProductTypeDrinkValue => ProductType.Drink,
            _ => ProductType.Food,
        };
    }

    private string MapProductTypeToValue(ProductType productType)
    {
        return productType switch
        {
            ProductType.Drink => ProductTypeDrinkValue,
            _ => ProductTypeFoodValue,
        };
    }

    private bool TryParseRequiredDecimal(string? text, out decimal value)
    {
        const NumberStyles styles = NumberStyles.Number;
        string trimmedText = text?.Trim() ?? string.Empty;

        return (decimal.TryParse(trimmedText, styles, LocalizationService.Culture, out value)
                || decimal.TryParse(trimmedText, styles, CultureInfo.InvariantCulture, out value))
               && value >= 0m;
    }

    private static string NormalizeEditableImageUri(string? imageUri)
    {
        return string.IsNullOrWhiteSpace(imageUri)
               || string.Equals(imageUri, DefaultImageUri, StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : imageUri;
    }

    private static void ReplaceOptions(
        ObservableCollection<LocalizationOptionModel> collection,
        IReadOnlyList<LocalizationOptionModel> options)
    {
        collection.Clear();
        foreach (LocalizationOptionModel option in options)
        {
            collection.Add(option);
        }
    }
}
