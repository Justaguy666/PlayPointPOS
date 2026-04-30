using Domain.Enums;
using WinUI.UIModels.Management;

namespace WinUI.Services.Factories;

public sealed class AreaDraftFactory
{
    public AreaModel Create()
    {
        return new AreaModel
        {
            Status = PlayAreaStatus.Available,
            PlayAreaType = PlayAreaType.Table,
            MaxCapacity = 2,
            HourlyPrice = 30000m,
        };
    }
}
