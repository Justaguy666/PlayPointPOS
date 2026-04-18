using Domain.Entities;
using Domain.Enums;

namespace Application.Games;

public sealed record BoardGameFilter
{
    public GameType? GameType { get; init; }

    public GameDifficulty? GameDifficulty { get; init; }

    public int? PlayerCount { get; init; }

    public decimal? HourlyPriceMin { get; init; }

    public decimal? HourlyPriceMax { get; init; }
}
