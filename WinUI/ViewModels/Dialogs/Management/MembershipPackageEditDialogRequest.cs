using System;
using System.Threading.Tasks;
using Windows.UI;

namespace WinUI.ViewModels.Dialogs.Management;

public sealed class MembershipPackageEditDialogRequest
{
    public required MembershipPackageItemViewModel Item { get; init; }

    public Func<MembershipPackageItemViewModel, string, string, string, Color, Task>? OnSubmittedAsync { get; init; }
}
