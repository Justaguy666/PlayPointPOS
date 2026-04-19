using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Domain.Enums;
using Microsoft.UI.Xaml.Media;
using WinUI.Resources;
using WinUI.UIModels;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;
using WinUI.ViewModels;

namespace WinUI.ViewModels.UserControls.Products;

public abstract partial class ProductCardControlViewModelBase : LocalizedViewModelBase
{
    private readonly Func<ProductModel, Task>? _editAction;
    private readonly Func<ProductModel, Task>? _deleteAction;
    private readonly Brush _foodBadgeBackgroundBrush;
    private readonly Brush _drinkBadgeBackgroundBrush;
    private readonly Brush _typeBadgeForegroundBrush;
    private bool _isDisposed;

    protected ProductCardControlViewModelBase(
        ILocalizationService localizationService,
        ProductModel model,
        Func<ProductModel, Task>? editAction,
        Func<ProductModel, Task>? deleteAction)
        : base(localizationService)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        _editAction = editAction;
        _deleteAction = deleteAction;

        _foodBadgeBackgroundBrush = AppResourceLookup.GetBrush("WarningAmberBrush", AppColors.WarningAmber);
        _drinkBadgeBackgroundBrush = AppResourceLookup.GetBrush("InfoBlueBrush", AppColors.InfoBlue);
        _typeBadgeForegroundBrush = AppResourceLookup.GetBrush("WhiteBrush", AppColors.White);

        EditCommand = new AsyncRelayCommand(ExecuteEditAsync);
        DeleteCommand = new AsyncRelayCommand(ExecuteDeleteAsync);

        Model.PropertyChanged += HandleModelPropertyChanged;
        RefreshLocalizedText();
    }

    public ProductModel Model { get; }

    [ObservableProperty]
    public partial string TypeDisplayName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PriceText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string PriceLabelText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial Brush TypeBadgeBackground { get; set; } = new SolidColorBrush(AppColors.InfoBlue);

    public string Name => Model.Name;

    public string ImageUri => Model.ImageUri;

    public Brush TypeBadgeForeground => _typeBadgeForegroundBrush;

    public IconState EditIconState { get; } = new()
    {
        Kind = IconKind.Update,
        Size = 20,
        AlwaysFilled = true,
    };

    public IconState DeleteIconState { get; } = new()
    {
        Kind = IconKind.Delete,
        Size = 20,
        AlwaysFilled = true,
    };

    public IAsyncRelayCommand EditCommand { get; }

    public IAsyncRelayCommand DeleteCommand { get; }

    protected override void RefreshLocalizedText()
    {
        TypeDisplayName = Model.ProductType switch
        {
            ProductType.Drink => LocalizationService.GetString("ProductTypeDrinkText"),
            _ => LocalizationService.GetString("ProductTypeFoodText"),
        };

        TypeBadgeBackground = Model.ProductType switch
        {
            ProductType.Drink => _drinkBadgeBackgroundBrush,
            _ => _foodBadgeBackgroundBrush,
        };

        PriceText = LocalizationService.FormatCurrency(Model.Price);
        PriceLabelText = LocalizationService.GetString("ProductCardControlPriceLabel");
        NotifyModelPresentationChanged();
    }

    public new void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        Model.PropertyChanged -= HandleModelPropertyChanged;
        _isDisposed = true;
        base.Dispose();
    }

    private void HandleModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ProductModel.Name):
                OnPropertyChanged(nameof(Name));
                break;
            case nameof(ProductModel.ImageUri):
                OnPropertyChanged(nameof(ImageUri));
                break;
            case nameof(ProductModel.Price):
            case nameof(ProductModel.ProductType):
                RefreshLocalizedText();
                break;
        }
    }

    private Task ExecuteEditAsync()
    {
        return _editAction?.Invoke(Model) ?? Task.CompletedTask;
    }

    private Task ExecuteDeleteAsync()
    {
        return _deleteAction?.Invoke(Model) ?? Task.CompletedTask;
    }

    private void NotifyModelPresentationChanged()
    {
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(ImageUri));
    }
}
