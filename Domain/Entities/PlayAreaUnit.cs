using Domain.Enums;

namespace Domain.Entities;

public class PlayAreaUnit : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PlayAreaType Type { get; set; } = PlayAreaType.Table;
    public PlayAreaStatus Status { get; set; } = PlayAreaStatus.Available;
    public int MaxOccupancy { get; set; }
    public int CurrentOccupancy { get; set; }
    public decimal HourlyPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public string? CurrentSessionId { get; set; }
    public string? CurrentReservationId { get; set; }
}
