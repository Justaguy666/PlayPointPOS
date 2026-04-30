using Application.Services.Games;

namespace Application.Games;

public sealed class GameTypeManagementService : IGameTypeManagementService
{
    public GameType Add(IList<GameType> gameTypes, string name)
    {
        ArgumentNullException.ThrowIfNull(gameTypes);

        string trimmedName = NormalizeName(name);
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            throw new ArgumentException("Game type name is required.", nameof(name));
        }

        GameType existingGameType = gameTypes.FirstOrDefault(type => IsSameName(type.Name, trimmedName))
            ?? new GameType { Name = trimmedName };

        if (!gameTypes.Contains(existingGameType))
        {
            gameTypes.Add(existingGameType);
        }

        return existingGameType;
    }

    public bool Delete(IList<GameType> gameTypes, GameType gameType)
    {
        ArgumentNullException.ThrowIfNull(gameTypes);
        ArgumentNullException.ThrowIfNull(gameType);

        GameType? target = gameTypes.FirstOrDefault(type => IsSame(type, gameType));
        return target is not null && gameTypes.Remove(target);
    }

    public bool Update(GameType gameType, string name)
    {
        ArgumentNullException.ThrowIfNull(gameType);

        string trimmedName = NormalizeName(name);
        if (string.IsNullOrWhiteSpace(trimmedName) || IsSameName(gameType.Name, trimmedName))
        {
            return false;
        }

        gameType.Name = trimmedName;
        return true;
    }

    public bool IsSame(GameType? left, GameType? right)
    {
        return left is not null
            && right is not null
            && IsSameName(left.Name, right.Name);
    }

    private static string NormalizeName(string? name)
    {
        return name?.Trim() ?? string.Empty;
    }

    private static bool IsSameName(string? left, string? right)
    {
        return string.Equals(
            NormalizeName(left),
            NormalizeName(right),
            StringComparison.OrdinalIgnoreCase);
    }
}
