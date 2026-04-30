using Domain.Enums;

namespace Application.Transactions;

public interface ITransactionFilterable
{
    PaymentMethod PaymentMethod { get; }

    decimal TotalAmount { get; }

    DateTime CreatedAt { get; }
}
