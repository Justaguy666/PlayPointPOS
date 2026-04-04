using Domain.Enums;

namespace Domain.Entities;

public class PlayAreaUnit : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public PlayAreaType Type { get; set; }
    public PlayAreaStatus Status { get; set; } = PlayAreaStatus.Available;
    public int MaxOccupancy { get; set; }
    public decimal HourlyPrice { get; set; }
}
