using System;
using Domain.Enums;

namespace WinUI.UIModels.AreaManagement.SummarizedAreaCards;

public sealed class SummarizedReservedCardModel : ISummarizedAreaCardModel
{
    public string AreaName { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public DateTime CheckInTime { get; set; }

    public int Capacity { get; set; }

    public PlayAreaStatus Status { get; set; } = PlayAreaStatus.Reserved;
}
