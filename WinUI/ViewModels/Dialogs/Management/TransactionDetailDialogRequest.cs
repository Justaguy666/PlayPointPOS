using System;
using System.Threading.Tasks;
using WinUI.UIModels.Management;

namespace WinUI.ViewModels.Dialogs.Management;

public sealed class TransactionDetailDialogRequest
{
    public required TransactionModel Model { get; init; }
}
