using Application.Games;

namespace Application.Services.Games;

public interface IGameTypeCatalogService
{
    IReadOnlyList<GameType> GetGameTypes();
}
