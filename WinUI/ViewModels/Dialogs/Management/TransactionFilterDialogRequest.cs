using System;
using System.Threading.Tasks;
using Application.Transactions;

namespace WinUI.ViewModels.Dialogs.Management;

public sealed class TransactionFilterDialogRequest
{
    public TransactionFilter InitialCriteria { get; set; } = new();
    public Func<TransactionFilter, Task>? OnSubmittedAsync { get; set; }
}
