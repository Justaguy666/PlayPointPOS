using System;
using System.Threading.Tasks;

namespace WinUI.ViewModels.Dialogs.Management;

public sealed class AreaFilterDialogRequest
{
    public AreaFilterCriteria? InitialCriteria { get; init; }

    public Func<AreaFilterCriteria, Task>? OnSubmittedAsync { get; init; }
}
