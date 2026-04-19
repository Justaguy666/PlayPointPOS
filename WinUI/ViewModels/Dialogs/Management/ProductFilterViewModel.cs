using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Application.Products;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Enums;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.ViewModels;

namespace WinUI.ViewModels.Dialogs.Management;

public partial class ProductFilterViewModel : LocalizedViewModelBase
{
    private const string AllOptionValue = "";
    private const string ProductTypeFoodValue = "food";
    private const string ProductTypeDrinkValue = "drink";

    private Func<ProductFilter, Task>? _onSubmittedAsync;
    private bool _isUpdatingSelectionOptions;
    private event Action? CloseRequestedInternal;

    public ProductFilterViewModel(ILocalizationService localizationService)
        : base(localizationService)
    {
        Icon = new IconState
        {
            Kind = IconKind.Filter,
            Size = 24,
            AlwaysFilled = true,
        };

        RefreshLocalizedText();
    }

    [ObservableProperty]
    public partial IconState Icon { get; set; } = new();

    [ObservableProperty]
    public partial string TitleText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ProductTypeLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PriceRangeLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PriceFromPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PriceToPlaceholderText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AllOptionsText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ResetButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ApplyButtonText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CloseTooltipText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApply))]
    public partial string? SelectedProductTypeValue { get; set; } = AllOptionValue;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApply))]
    public partial string PriceMinText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApply))]
    public partial string PriceMaxText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string ErrorMessage { get; set; } = string.Empty;

    public ObservableCollection<LocalizationOptionModel> ProductTypeOptions { get; } = [];

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool CanApply => HasValidNumericRanges();

    public event Action? CloseRequested
    {
        add => CloseRequestedInternal += value;
        remove => CloseRequestedInternal -= value;
    }

    public void Configure(ProductFilterDialogRequest? request)
    {
        _onSubmittedAsync = request?.OnSubmittedAsync;

        ProductFilter criteria = request?.InitialCriteria ?? new ProductFilter();
        SelectedProductTypeValue = MapProductTypeToValue(criteria.ProductType);
        PriceMinText = criteria.PriceMin?.ToString("0.##", LocalizationService.Culture) ?? string.Empty;
        PriceMaxText = criteria.PriceMax?.ToString("0.##", LocalizationService.Culture) ?? string.Empty;
        ErrorMessage = string.Empty;

        RefreshSelectionOptions();
    }

    protected override void RefreshLocalizedText()
    {
        TitleText = LocalizationService.GetString("ProductFilterDialogTitleText");
        ProductTypeLabelText = LocalizationService.GetString("ProductFilterDialogProductTypeLabelText");
        PriceRangeLabelText = LocalizationService.GetString("ProductFilterDialogPriceRangeLabelText");
        PriceFromPlaceholderText = LocalizationService.GetString("ProductFilterDialogPriceFromPlaceholderText");
        PriceToPlaceholderText = LocalizationService.GetString("ProductFilterDialogPriceToPlaceholderText");
        AllOptionsText = LocalizationService.GetString("ProductFilterDialogAllOptionsText");
        ResetButtonText = LocalizationService.GetString("ProductFilterDialogResetButtonText");
        ApplyButtonText = LocalizationService.GetString("ProductFilterDialogApplyButtonText");
        CloseTooltipText = LocalizationService.GetString("CloseTooltipText");

        RefreshSelectionOptions();
    }

    [RelayCommand(CanExecute = nameof(CanApply))]
    private async Task ApplyAsync()
    {
        if (!TryBuildCriteria(out ProductFilter criteria))
        {
            ErrorMessage = LocalizationService.GetString("ProductFilterDialogInvalidRangeText");
            return;
        }

        ErrorMessage = string.Empty;

        if (_onSubmittedAsync is not null)
        {
            await _onSubmittedAsync(criteria);
        }

        CloseRequestedInternal?.Invoke();
    }

    [RelayCommand]
    private void Reset()
    {
        SelectedProductTypeValue = AllOptionValue;
        PriceMinText = string.Empty;
        PriceMaxText = string.Empty;
        ErrorMessage = string.Empty;
        NotifyFilterInputChanged();
    }

    [RelayCommand]
    private void Close()
    {
        CloseRequestedInternal?.Invoke();
    }

    partial void OnSelectedProductTypeValueChanged(string? value)
    {
        if (!_isUpdatingSelectionOptions)
        {
            NotifyFilterInputChanged();
        }
    }

    partial void OnPriceMinTextChanged(string value) => NotifyFilterInputChanged();

    partial void OnPriceMaxTextChanged(string value) => NotifyFilterInputChanged();

    private void RefreshSelectionOptions()
    {
        _isUpdatingSelectionOptions = true;

        try
        {
            string currentProductTypeValue = SelectedProductTypeValue ?? AllOptionValue;

            ReplaceOptions(
                ProductTypeOptions,
                [
                    new LocalizationOptionModel { Value = AllOptionValue, DisplayName = AllOptionsText },
                    new LocalizationOptionModel { Value = ProductTypeFoodValue, DisplayName = LocalizationService.GetString("ProductTypeFoodText") },
                    new LocalizationOptionModel { Value = ProductTypeDrinkValue, DisplayName = LocalizationService.GetString("ProductTypeDrinkText") },
                ]);

            if (!ProductTypeOptions.Any(option => option.Value == currentProductTypeValue))
            {
                currentProductTypeValue = AllOptionValue;
            }

            SelectedProductTypeValue = currentProductTypeValue;
        }
        finally
        {
            _isUpdatingSelectionOptions = false;
        }
    }

    private void NotifyFilterInputChanged()
    {
        if (!string.IsNullOrWhiteSpace(ErrorMessage))
        {
            ErrorMessage = string.Empty;
        }

        ApplyCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(CanApply));
    }

    private bool HasValidNumericRanges()
    {
        return TryParseOptionalDecimal(PriceMinText, out _) && TryParseOptionalDecimal(PriceMaxText, out _);
    }

    private bool TryBuildCriteria(out ProductFilter criteria)
    {
        criteria = new ProductFilter();

        if (!TryParseOptionalDecimal(PriceMinText, out decimal? priceMin)
            || !TryParseOptionalDecimal(PriceMaxText, out decimal? priceMax))
        {
            return false;
        }

        (decimal? normalizedPriceMin, decimal? normalizedPriceMax) = NormalizeRange(priceMin, priceMax);

        criteria = new ProductFilter
        {
            ProductType = ResolveSelectedProductType(),
            PriceMin = normalizedPriceMin,
            PriceMax = normalizedPriceMax,
        };

        return true;
    }

    private ProductType? ResolveSelectedProductType()
    {
        return SelectedProductTypeValue switch
        {
            ProductTypeFoodValue => ProductType.Food,
            ProductTypeDrinkValue => ProductType.Drink,
            _ => null,
        };
    }

    private string MapProductTypeToValue(ProductType? productType)
    {
        return productType switch
        {
            ProductType.Food => ProductTypeFoodValue,
            ProductType.Drink => ProductTypeDrinkValue,
            _ => AllOptionValue,
        };
    }

    private bool TryParseOptionalDecimal(string? text, out decimal? value)
    {
        const NumberStyles styles = NumberStyles.Number;
        string trimmedText = text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmedText))
        {
            value = null;
            return true;
        }

        bool success =
            decimal.TryParse(trimmedText, styles, LocalizationService.Culture, out decimal parsedValue)
            || decimal.TryParse(trimmedText, styles, CultureInfo.InvariantCulture, out parsedValue);

        value = success ? Math.Max(0m, parsedValue) : null;
        return success;
    }

    private static (T? Min, T? Max) NormalizeRange<T>(T? min, T? max)
        where T : struct, IComparable<T>
    {
        if (min.HasValue && max.HasValue && min.Value.CompareTo(max.Value) > 0)
        {
            return (max, min);
        }

        return (min, max);
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
