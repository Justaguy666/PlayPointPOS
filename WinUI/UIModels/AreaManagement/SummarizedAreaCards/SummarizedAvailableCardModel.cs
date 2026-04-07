using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace WinUI.UIModels.AreaManagement.SummarizedAreaCards;

public class SummarizedAvailableCardModel : ISummarizedAreaCardModel
{
    public string AreaName { get; set; } = string.Empty;
    public PlayAreaStatus Status { get; set; } = PlayAreaStatus.Available;
}
