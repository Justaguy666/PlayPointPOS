using System;
using Application.Services.Games;

namespace Application.Games;

public sealed class GameFilterService : IGameFilterService
{
    public IReadOnlyList<TGame> Apply<TGame>(IEnumerable<TGame> games, BoardGameFilter filter)
        where TGame : IGameFilterable
    {
        ArgumentNullException.ThrowIfNull(games);
        ArgumentNullException.ThrowIfNull(filter);

        return games
            .Where(game =>
                MatchesGameType(game, filter)
                && MatchesGameDifficulty(game, filter)
                && MatchesPlayerCount(game, filter)
                && MatchesHourlyPrice(game, filter))
            .ToList();
    }

    private static bool MatchesGameType(IGameFilterable game, BoardGameFilter filter)
    {
        return filter.GameType is null
            || string.Equals(
                game.GameType?.Name,
                filter.GameType.Name,
                StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesGameDifficulty(IGameFilterable game, BoardGameFilter filter)
    {
        return filter.GameDifficulty is null || game.GameDifficulty == filter.GameDifficulty.Value;
    }

    private static bool MatchesPlayerCount(IGameFilterable game, BoardGameFilter filter)
    {
        return filter.PlayerCount is null
            || (game.MinPlayers <= filter.PlayerCount.Value && game.MaxPlayers >= filter.PlayerCount.Value);
    }

    private static bool MatchesHourlyPrice(IGameFilterable game, BoardGameFilter filter)
    {
        return (filter.HourlyPriceMin is null || game.HourlyPrice >= filter.HourlyPriceMin.Value)
            && (filter.HourlyPriceMax is null || game.HourlyPrice <= filter.HourlyPriceMax.Value);
    }
}
