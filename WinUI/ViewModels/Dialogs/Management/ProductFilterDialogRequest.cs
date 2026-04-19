using System;
using System.Threading.Tasks;
using Application.Products;

namespace WinUI.ViewModels.Dialogs.Management;

public sealed class ProductFilterDialogRequest
{
    public ProductFilter? InitialCriteria { get; init; }

    public Func<ProductFilter, Task>? OnSubmittedAsync { get; init; }
}
