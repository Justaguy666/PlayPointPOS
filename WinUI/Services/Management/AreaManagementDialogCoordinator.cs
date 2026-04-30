using System;
using System.Threading.Tasks;
using Application.Services;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;
using WinUI.ViewModels.Dialogs.Management;

namespace WinUI.Services.Management;

public sealed class AreaManagementDialogCoordinator
{
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;

    public AreaManagementDialogCoordinator(
        IDialogService dialogService,
        ILocalizationService localizationService,
        INotificationService notificationService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public Task OpenFilterAsync(AreaFilterCriteria initialCriteria, Func<AreaFilterCriteria, Task> onSubmittedAsync)
    {
        return _dialogService.ShowDialogAsync(
            new AreaFilterDialogRequest
            {
                InitialCriteria = initialCriteria,
                OnSubmittedAsync = onSubmittedAsync,
            });
    }

    public Task OpenAddAsync(AreaModel draft, Func<AreaModel, Task> onSubmittedAsync)
    {
        return OpenUpsertAsync(UpsertDialogMode.Add, draft, onSubmittedAsync);
    }

    public Task OpenEditAsync(AreaModel area, Func<AreaModel, Task> onSubmittedAsync)
    {
        return OpenUpsertAsync(UpsertDialogMode.Edit, area, onSubmittedAsync);
    }

    public Task<bool> ConfirmDeleteAsync()
    {
        return _dialogService.ShowConfirmationAsync(
            titleKey: "ConfirmDeleteAreaTitle",
            messageKey: "ConfirmDeleteAreaMessage",
            confirmButtonTextKey: "ConfirmDeleteAreaButton",
            cancelButtonTextKey: "CancelButtonText");
    }

    public Task NotifyDeletedAsync(AreaModel area)
    {
        return _notificationService.SendAsync(
            _localizationService.GetString("AreaDeletedSuccessTitle"),
            string.Format(
                _localizationService.Culture,
                _localizationService.GetString("AreaDeletedSuccessMessage"),
                area.AreaName),
            NotificationType.Success);
    }

    private Task OpenUpsertAsync(UpsertDialogMode mode, AreaModel area, Func<AreaModel, Task> onSubmittedAsync)
    {
        return _dialogService.ShowDialogAsync(
            new AreaDialogRequest
            {
                Mode = mode,
                Model = area,
                OnSubmittedAsync = onSubmittedAsync,
            });
    }
}
