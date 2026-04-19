using System;
using System.Threading.Tasks;
using Application.Services;
using WinUI.UIModels.Management;
using WinUI.ViewModels.UserControls.Products;

namespace WinUI.Services.Factories;

public sealed class ProductCardControlViewModelFactory
{
    private readonly ILocalizationService _localizationService;

    public ProductCardControlViewModelFactory(ILocalizationService localizationService)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }

    public GridProductCardControlViewModel CreateGrid(
        ProductModel model,
        Func<ProductModel, Task>? editAction,
        Func<ProductModel, Task>? deleteAction)
    {
        return new GridProductCardControlViewModel(
            _localizationService,
            model,
            editAction,
            deleteAction);
    }

    public ListProductCardControlViewModel CreateList(
        ProductModel model,
        Func<ProductModel, Task>? editAction,
        Func<ProductModel, Task>? deleteAction)
    {
        return new ListProductCardControlViewModel(
            _localizationService,
            model,
            editAction,
            deleteAction);
    }
}
