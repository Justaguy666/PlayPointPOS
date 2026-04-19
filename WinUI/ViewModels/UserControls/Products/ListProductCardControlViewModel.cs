using System;
using System.Threading.Tasks;
using Application.Services;
using WinUI.UIModels.Management;

namespace WinUI.ViewModels.UserControls.Products;

public sealed class ListProductCardControlViewModel : ProductCardControlViewModelBase
{
    public ListProductCardControlViewModel(
        ILocalizationService localizationService,
        ProductModel model,
        Func<ProductModel, Task>? editAction,
        Func<ProductModel, Task>? deleteAction)
        : base(
            localizationService,
            model,
            editAction,
            deleteAction)
    {
    }
}
