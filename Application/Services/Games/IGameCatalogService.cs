using Application.Games;

namespace Application.Services.Games;

public interface IGameCatalogService
{
    IReadOnlyList<GameRecord> GetGames();
}
