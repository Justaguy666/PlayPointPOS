using System;
using System.Threading.Tasks;
using Application.Services;
using WinUI.UIModels.Management;

namespace WinUI.ViewModels.UserControls.Games;

public sealed class GridGameCardControlViewModel : GameCardControlViewModelBase
{
    public GridGameCardControlViewModel(
        ILocalizationService localizationService,
        GameModel model,
        Func<GameModel, Task>? editAction,
        Func<GameModel, Task>? deleteAction,
        Action<GameModel>? increaseStockAction,
        Action<GameModel>? decreaseStockAction)
        : base(
            localizationService,
            model,
            editAction,
            deleteAction,
            increaseStockAction,
            decreaseStockAction)
    {
    }
}
