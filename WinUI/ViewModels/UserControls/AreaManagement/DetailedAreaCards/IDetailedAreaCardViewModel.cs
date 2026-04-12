using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace WinUI.ViewModels.AreaManagement.DetailedAreaCards;

public interface IDetailedAreaCardViewModel
{
    public string AreaName { get; }
    public string MaxCapacityText { get; }
}
