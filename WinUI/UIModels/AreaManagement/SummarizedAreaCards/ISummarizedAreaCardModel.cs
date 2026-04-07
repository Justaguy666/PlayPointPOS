using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace WinUI.UIModels.AreaManagement.SummarizedAreaCards;

public interface ISummarizedAreaCardModel
{
    public string AreaName { get; }
    public PlayAreaStatus Status { get; }
}
