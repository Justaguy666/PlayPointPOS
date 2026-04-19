using Domain.Enums;

namespace Application.Transactions;

public sealed class TransactionFilter
{
    public PaymentMethod? PaymentMethod { get; set; }
    public decimal? AmountMin { get; set; }
    public decimal? AmountMax { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
