using System;
using Domain.Enums;

namespace WinUI.ViewModels.Dialogs.Management;

public sealed class AreaFilterCriteria
{
    public PlayAreaStatus? Status { get; init; }

    public TimeSpan? StartTimeFrom { get; init; }

    public TimeSpan? StartTimeTo { get; init; }

    public int? CapacityMin { get; init; }

    public int? CapacityMax { get; init; }

    public decimal? HourlyPriceMin { get; init; }

    public decimal? HourlyPriceMax { get; init; }
}
