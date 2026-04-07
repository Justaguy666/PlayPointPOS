using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace WinUI.UIModels.AreaManagement.DetailedAreaCards;

public interface IDetailedAreaCardModel
{
    public string AreaName { get; }
    public PlayAreaStatus PlayAreaStatus { get; }
    public int Capacity { get; }
}
