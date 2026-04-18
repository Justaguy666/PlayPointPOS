using Application.Games;

namespace Application.Services.Games;

public interface IGameFilterService
{
    IReadOnlyList<TGame> Apply<TGame>(IEnumerable<TGame> games, BoardGameFilter filter)
        where TGame : IGameFilterable;
}
