using System;
using System.Threading.Tasks;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;

namespace WinUI.ViewModels.Dialogs.Management;

public sealed class ProductDialogRequest
{
    public UpsertDialogMode Mode { get; init; } = UpsertDialogMode.Add;

    public ProductModel? Model { get; init; }

    public Func<ProductModel, Task>? OnSubmittedAsync { get; init; }
}
