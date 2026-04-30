using System;
using System.Threading.Tasks;
using Application.Services;
using Application.Transactions;
using WinUI.UIModels.Management;
using WinUI.ViewModels.Dialogs.Management;

namespace WinUI.Services.Management;

public sealed class TransactionManagementDialogCoordinator
{
    private readonly IDialogService _dialogService;

    public TransactionManagementDialogCoordinator(IDialogService dialogService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    }

    public Task OpenFilterAsync(TransactionFilter initialCriteria, Func<TransactionFilter, Task> onSubmittedAsync)
    {
        return _dialogService.ShowDialogAsync(
            DialogKey.TransactionFilter,
            new TransactionFilterDialogRequest
            {
                InitialCriteria = initialCriteria,
                OnSubmittedAsync = onSubmittedAsync,
            });
    }

    public Task OpenDetailAsync(TransactionModel transaction)
    {
        return _dialogService.ShowDialogAsync(
            DialogKey.TransactionDetail,
            new TransactionDetailDialogRequest { Model = transaction });
    }
}
