using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Application.Games;
using Application.Services;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;
using WinUI.ViewModels.Dialogs.Management;

namespace WinUI.Services.Management;

public sealed class GameManagementDialogCoordinator
{
    private readonly IDialogService _dialogService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;

    public GameManagementDialogCoordinator(
        IDialogService dialogService,
        ILocalizationService localizationService,
        INotificationService notificationService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public Task OpenFilterAsync(
        IReadOnlyList<GameType> availableGameTypes,
        BoardGameFilter initialCriteria,
        Func<BoardGameFilter, Task> onSubmittedAsync)
    {
        return _dialogService.ShowDialogAsync(
            DialogKey.GameFilter,
            new GameFilterDialogRequest
            {
                AvailableGameTypes = availableGameTypes,
                InitialCriteria = initialCriteria,
                OnSubmittedAsync = onSubmittedAsync,
            });
    }

    public Task OpenAddAsync(
        GameModel draft,
        IReadOnlyList<GameType> availableGameTypes,
        Func<GameModel, Task> onSubmittedAsync)
    {
        return OpenUpsertAsync(UpsertDialogMode.Add, draft, availableGameTypes, onSubmittedAsync);
    }

    public Task OpenEditAsync(
        GameModel game,
        IReadOnlyList<GameType> availableGameTypes,
        Func<GameModel, Task> onSubmittedAsync)
    {
        return OpenUpsertAsync(UpsertDialogMode.Edit, game, availableGameTypes, onSubmittedAsync);
    }

    public Task OpenGameTypesAsync(
        ObservableCollection<GameType> gameTypes,
        Func<GameType, Task> onGameTypeAddedAsync,
        Func<GameType, Task> onGameTypeDeletedAsync,
        Func<GameType, Task> onGameTypeUpdatedAsync)
    {
        return _dialogService.ShowDialogAsync(
            DialogKey.GameType,
            new GameTypeDialogRequest
            {
                GameTypes = gameTypes,
                OnGameTypeAddedAsync = onGameTypeAddedAsync,
                OnGameTypeDeletedAsync = onGameTypeDeletedAsync,
                OnGameTypeUpdatedAsync = onGameTypeUpdatedAsync,
            });
    }

    public Task<bool> ConfirmDeleteAsync()
    {
        return _dialogService.ShowConfirmationAsync(
            titleKey: "ConfirmDeleteGameTitle",
            messageKey: "ConfirmDeleteGameMessage",
            confirmButtonTextKey: "ConfirmDeleteGameButton",
            cancelButtonTextKey: "CancelButtonText");
    }

    public Task NotifyCreatedAsync(GameModel game)
    {
        return NotifyAsync("GameCreatedSuccessTitle", "GameCreatedSuccessMessage", game.Name);
    }

    public Task NotifyUpdatedAsync(GameModel game)
    {
        return NotifyAsync("GameUpdatedSuccessTitle", "GameUpdatedSuccessMessage", game.Name);
    }

    public Task NotifyDeletedAsync(GameModel game)
    {
        return NotifyAsync("GameDeletedSuccessTitle", "GameDeletedSuccessMessage", game.Name);
    }

    private Task OpenUpsertAsync(
        UpsertDialogMode mode,
        GameModel game,
        IReadOnlyList<GameType> availableGameTypes,
        Func<GameModel, Task> onSubmittedAsync)
    {
        return _dialogService.ShowDialogAsync(
            DialogKey.Game,
            new GameDialogRequest
            {
                Mode = mode,
                Model = game,
                AvailableGameTypes = availableGameTypes,
                OnSubmittedAsync = onSubmittedAsync,
            });
    }

    private Task NotifyAsync(string titleKey, string messageKey, string name)
    {
        return _notificationService.SendAsync(
            _localizationService.GetString(titleKey),
            string.Format(
                _localizationService.Culture,
                _localizationService.GetString(messageKey),
                name),
            NotificationType.Success);
    }
}
