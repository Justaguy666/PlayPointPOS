using Domain.Enums;

namespace Domain.Entities;

public class PlayAreaSession : BaseEntity
{
    public string PlayAreaUnitId { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string? MemberId { get; set; }
    public string? ReservationId { get; set; }
    public PlayAreaSessionStatus Status { get; set; } = PlayAreaSessionStatus.Active;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public int CurrentOccupancy { get; set; }
    public decimal HourlyPriceSnapshot { get; set; }
    public decimal AreaCharge { get; set; }
}
