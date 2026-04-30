using Application.Games;

namespace Application.Services.Games;

public interface IGameTypeManagementService
{
    GameType Add(IList<GameType> gameTypes, string name);

    bool Delete(IList<GameType> gameTypes, GameType gameType);

    bool Update(GameType gameType, string name);

    bool IsSame(GameType? left, GameType? right);
}
