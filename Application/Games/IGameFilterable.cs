using Domain.Entities;
using Domain.Enums;

namespace Application.Games;

public interface IGameFilterable
{
    GameType GameType { get; }

    GameDifficulty GameDifficulty { get; }

    int MinPlayers { get; }

    int MaxPlayers { get; }

    decimal HourlyPrice { get; }
}
