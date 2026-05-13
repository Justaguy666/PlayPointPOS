using Domain.Enums;

namespace Application.Transactions;

public sealed record AreaSessionCheckoutExtra(string Kind, int CatalogId, decimal Quantity, decimal UnitPrice);

public sealed record AreaSessionCheckoutArgs(
    int AreaId,
    int SessionId,
    PaymentMethod PaymentMethod,
    decimal AreaServiceCharge,
    IReadOnlyList<AreaSessionCheckoutExtra> Extras);
