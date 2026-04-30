using System;
using System.Threading.Tasks;
using Application.Services;
using Application.Products;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;
using WinUI.ViewModels.Dialogs.Management;

namespace WinUI.Services.Management;

public sealed class ProductManagementDialogCoordinator
{
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;

    public ProductManagementDialogCoordinator(
        IDialogService dialogService,
        ILocalizationService localizationService,
        INotificationService notificationService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public Task OpenFilterAsync(ProductFilter initialCriteria, Func<ProductFilter, Task> onSubmittedAsync)
    {
        return _dialogService.ShowDialogAsync(
            DialogKey.ProductFilter,
            new ProductFilterDialogRequest
            {
                InitialCriteria = initialCriteria,
                OnSubmittedAsync = onSubmittedAsync,
            });
    }

    public Task OpenAddAsync(ProductModel draft, Func<ProductModel, Task> onSubmittedAsync)
    {
        return OpenUpsertAsync(UpsertDialogMode.Add, draft, onSubmittedAsync);
    }

    public Task OpenEditAsync(ProductModel product, Func<ProductModel, Task> onSubmittedAsync)
    {
        return OpenUpsertAsync(UpsertDialogMode.Edit, product, onSubmittedAsync);
    }

    public Task<bool> ConfirmDeleteAsync()
    {
        return _dialogService.ShowConfirmationAsync(
            titleKey: "ConfirmDeleteProductTitle",
            messageKey: "ConfirmDeleteProductMessage",
            confirmButtonTextKey: "ConfirmDeleteProductButton",
            cancelButtonTextKey: "CancelButtonText");
    }

    public Task NotifyCreatedAsync(ProductModel product)
    {
        return NotifyAsync("ProductCreatedSuccessTitle", "ProductCreatedSuccessMessage", product.Name);
    }

    public Task NotifyUpdatedAsync(ProductModel product)
    {
        return NotifyAsync("ProductUpdatedSuccessTitle", "ProductUpdatedSuccessMessage", product.Name);
    }

    public Task NotifyDeletedAsync(ProductModel product)
    {
        return NotifyAsync("ProductDeletedSuccessTitle", "ProductDeletedSuccessMessage", product.Name);
    }

    private Task OpenUpsertAsync(UpsertDialogMode mode, ProductModel product, Func<ProductModel, Task> onSubmittedAsync)
    {
        return _dialogService.ShowDialogAsync(
            DialogKey.Product,
            new ProductDialogRequest
            {
                Mode = mode,
                Model = product,
                OnSubmittedAsync = onSubmittedAsync,
            });
    }

    private Task NotifyAsync(string titleKey, string messageKey, string name)
    {
        return _notificationService.SendAsync(
            _localizationService.GetString(titleKey),
            string.Format(
                _localizationService.Culture,
                _localizationService.GetString(messageKey),
                name),
            NotificationType.Success);
    }
}
