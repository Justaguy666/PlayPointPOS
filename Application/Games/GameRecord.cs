using Domain.Entities;
using Domain.Enums;

namespace Application.Games;

public sealed record GameRecord
{
    public string Name { get; set; } = string.Empty;
    public decimal HourlyPrice { get; set; }
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public required GameType Type { get; set; }
    public GameDifficulty Difficulty { get; set; }
    public int StockQuantity { get; set; }
}
