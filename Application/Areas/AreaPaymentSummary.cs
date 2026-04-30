namespace Application.Areas;

public sealed record AreaPaymentSummary
{
    public TimeSpan ElapsedTime { get; init; }

    public decimal AreaFee { get; init; }

    public decimal ProductFee { get; init; }

    public decimal GameFee { get; init; }

    public decimal Deposit { get; init; }

    public decimal Discount { get; init; }

    public decimal Total { get; init; }
}
