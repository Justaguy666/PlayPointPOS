using Domain.Entities;
using System.Collections.Generic;

namespace Application.Services.Games;

public interface IGameTypeCatalogService
{
    IReadOnlyList<GameType> GetGameTypes();
}
