using Domain.Enums;

namespace Domain.Entities;

public class PlayAreaReservation : BaseEntity
{
    public string PlayAreaUnitId { get; set; } = string.Empty;
    public string? MemberId { get; set; }
    public string? SessionId { get; set; }
    public PlayAreaReservationStatus Status { get; set; } = PlayAreaReservationStatus.Reserved;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public int PartySize { get; set; }
    public DateTime ReservedFrom { get; set; }
    public DateTime ReservedUntil { get; set; }
    public string Notes { get; set; } = string.Empty;
}
