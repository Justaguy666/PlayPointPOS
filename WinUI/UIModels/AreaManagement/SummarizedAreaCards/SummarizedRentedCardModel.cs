using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace WinUI.UIModels.AreaManagement.SummarizedAreaCards;

public class SummarizedRentedCardModel : ISummarizedAreaCardModel
{
    public string AreaName { get; set; } = string.Empty;
    public PlayAreaStatus Status { get; set; } = PlayAreaStatus.Rented;
    public int Capacity { get; set; }
    public DateTime StartTime { get; set; }
    public decimal TotalAmount { get; set; }
}
