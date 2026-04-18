using Domain.Enums;

namespace Domain.Entities;

public class BoardGame : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal HourlyPrice { get; set; }
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public required GameType Type { get; set; }
    public GameDifficulty Difficulty { get; set; }
    public int StockQuantity { get; set; }
}
