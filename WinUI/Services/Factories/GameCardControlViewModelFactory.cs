using System;
using System.Threading.Tasks;
using Application.Services;
using WinUI.UIModels.Management;
using WinUI.ViewModels.UserControls.Games;

namespace WinUI.Services.Factories;

public sealed class GameCardControlViewModelFactory
{
    private readonly ILocalizationService _localizationService;

    public GameCardControlViewModelFactory(ILocalizationService localizationService)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }

    public GridGameCardControlViewModel CreateGrid(
        GameModel model,
        Func<GameModel, Task>? editAction,
        Func<GameModel, Task>? deleteAction,
        Action<GameModel>? increaseStockAction,
        Action<GameModel>? decreaseStockAction)
    {
        return new GridGameCardControlViewModel(
            _localizationService,
            model,
            editAction,
            deleteAction,
            increaseStockAction,
            decreaseStockAction);
    }

    public ListGameCardControlViewModel CreateList(
        GameModel model,
        Func<GameModel, Task>? editAction,
        Func<GameModel, Task>? deleteAction,
        Action<GameModel>? increaseStockAction,
        Action<GameModel>? decreaseStockAction)
    {
        return new ListGameCardControlViewModel(
            _localizationService,
            model,
            editAction,
            deleteAction,
            increaseStockAction,
            decreaseStockAction);
    }
}
