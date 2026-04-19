using Application.Services;
using System;
using System.Threading.Tasks;
using WinUI.UIModels.Management;
using WinUI.ViewModels.UserControls.Transactions;

namespace WinUI.Services.Factories;

public sealed class TransactionCardControlViewModelFactory
{
    private readonly ILocalizationService _localizationService;

    public TransactionCardControlViewModelFactory(ILocalizationService localizationService)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }

    public TransactionCardControlViewModel Create(
        TransactionModel model,
        Func<TransactionModel, Task>? showDetailAction,
        Func<TransactionModel, Task>? togglePaymentMethodAction)
    {
        return new TransactionCardControlViewModel(
            _localizationService,
            model,
            showDetailAction,
            togglePaymentMethodAction);
    }
}
