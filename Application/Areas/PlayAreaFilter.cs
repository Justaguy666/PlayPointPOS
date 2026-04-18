using Domain.Enums;

namespace Application.Areas;

public sealed record PlayAreaFilter
{
    public PlayAreaType? AreaType { get; init; }

    public PlayAreaStatus? Status { get; init; }

    public TimeSpan? StartTimeFrom { get; init; }

    public TimeSpan? StartTimeTo { get; init; }

    public int? CapacityMin { get; init; }

    public int? CapacityMax { get; init; }

    public decimal? HourlyPriceMin { get; init; }

    public decimal? HourlyPriceMax { get; init; }
}
