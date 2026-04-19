using Application.Services.Transactions;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Services.Transactions;

public sealed class MockTransactionCatalogService : ITransactionCatalogService
{
    private readonly List<Transaction> _transactions;

    public MockTransactionCatalogService()
    {
        _transactions = GenerateMockTransactions();
    }

    public IReadOnlyList<Transaction> GetTransactions() => _transactions.AsReadOnly();

    private static List<Transaction> GenerateMockTransactions()
    {
        return
        [
            new Transaction
            {
                Code = "TXN001",
                AccountId = "ACC001",
                MemberId = "#0001",
                PaymentMethod = PaymentMethod.Cash,
                SubtotalAmount = 285_000m,
                DiscountAmount = 13_000m,
                TotalAmount = 242_000m,
                CreatedAt = new DateTime(2024, 3, 15, 14, 30, 0),
                Lines =
                [
                    new TransactionLine { ItemName = "Thuê bàn 01", Type = TransactionLineType.AreaRental, Quantity = 3, UnitPrice = 10_000m, TotalAmount = 30_000m },
                    new TransactionLine { ItemName = "Catan", Type = TransactionLineType.BoardGameRental, Quantity = 3, UnitPrice = 20_000m, TotalAmount = 60_000m },
                    new TransactionLine { ItemName = "Trà sữa trân trâu", Type = TransactionLineType.ProductSale, Quantity = 3, UnitPrice = 35_000m, TotalAmount = 105_000m },
                    new TransactionLine { ItemName = "Bánh ngọt", Type = TransactionLineType.ProductSale, Quantity = 3, UnitPrice = 30_000m, TotalAmount = 90_000m },
                ],
            },
            new Transaction
            {
                Code = "TXN002",
                AccountId = "ACC001",
                MemberId = "#0002",
                PaymentMethod = PaymentMethod.Banking,
                SubtotalAmount = 450_000m,
                DiscountAmount = 0m,
                TotalAmount = 450_000m,
                CreatedAt = new DateTime(2024, 3, 15, 15, 45, 0),
                Lines =
                [
                    new TransactionLine { ItemName = "Thuê phòng VIP", Type = TransactionLineType.AreaRental, Quantity = 2, UnitPrice = 80_000m, TotalAmount = 160_000m },
                    new TransactionLine { ItemName = "Monopoly", Type = TransactionLineType.BoardGameRental, Quantity = 2, UnitPrice = 25_000m, TotalAmount = 50_000m },
                    new TransactionLine { ItemName = "Cà phê đen", Type = TransactionLineType.ProductSale, Quantity = 4, UnitPrice = 25_000m, TotalAmount = 100_000m },
                    new TransactionLine { ItemName = "Pizza", Type = TransactionLineType.ProductSale, Quantity = 2, UnitPrice = 70_000m, TotalAmount = 140_000m },
                ],
            },
            new Transaction
            {
                Code = "TXN003",
                AccountId = "ACC001",
                MemberId = "#0003",
                PaymentMethod = PaymentMethod.Cash,
                SubtotalAmount = 320_000m,
                DiscountAmount = 16_000m,
                TotalAmount = 304_000m,
                CreatedAt = new DateTime(2024, 3, 14, 10, 15, 0),
                Lines =
                [
                    new TransactionLine { ItemName = "Thuê bàn 03", Type = TransactionLineType.AreaRental, Quantity = 4, UnitPrice = 10_000m, TotalAmount = 40_000m },
                    new TransactionLine { ItemName = "Uno", Type = TransactionLineType.BoardGameRental, Quantity = 4, UnitPrice = 15_000m, TotalAmount = 60_000m },
                    new TransactionLine { ItemName = "Nước cam", Type = TransactionLineType.ProductSale, Quantity = 4, UnitPrice = 30_000m, TotalAmount = 120_000m },
                    new TransactionLine { ItemName = "Khoai tây chiên", Type = TransactionLineType.ProductSale, Quantity = 2, UnitPrice = 50_000m, TotalAmount = 100_000m },
                ],
            },
            new Transaction
            {
                Code = "TXN004",
                AccountId = "ACC001",
                MemberId = "#0001",
                PaymentMethod = PaymentMethod.Banking,
                SubtotalAmount = 180_000m,
                DiscountAmount = 9_000m,
                TotalAmount = 171_000m,
                CreatedAt = new DateTime(2024, 3, 14, 16, 0, 0),
                Lines =
                [
                    new TransactionLine { ItemName = "Thuê bàn 02", Type = TransactionLineType.AreaRental, Quantity = 2, UnitPrice = 10_000m, TotalAmount = 20_000m },
                    new TransactionLine { ItemName = "Ticket to Ride", Type = TransactionLineType.BoardGameRental, Quantity = 2, UnitPrice = 30_000m, TotalAmount = 60_000m },
                    new TransactionLine { ItemName = "Trà đào", Type = TransactionLineType.ProductSale, Quantity = 2, UnitPrice = 35_000m, TotalAmount = 70_000m },
                    new TransactionLine { ItemName = "Cheese Corn Dog", Type = TransactionLineType.ProductSale, Quantity = 1, UnitPrice = 48_000m, TotalAmount = 48_000m },
                ],
            },
            new Transaction
            {
                Code = "TXN005",
                AccountId = "ACC001",
                MemberId = "#0004",
                PaymentMethod = PaymentMethod.Cash,
                SubtotalAmount = 550_000m,
                DiscountAmount = 0m,
                TotalAmount = 550_000m,
                CreatedAt = new DateTime(2024, 3, 13, 19, 30, 0),
                Lines =
                [
                    new TransactionLine { ItemName = "Thuê phòng VIP", Type = TransactionLineType.AreaRental, Quantity = 3, UnitPrice = 80_000m, TotalAmount = 240_000m },
                    new TransactionLine { ItemName = "Catan", Type = TransactionLineType.BoardGameRental, Quantity = 3, UnitPrice = 20_000m, TotalAmount = 60_000m },
                    new TransactionLine { ItemName = "Trà sữa trân trâu", Type = TransactionLineType.ProductSale, Quantity = 5, UnitPrice = 35_000m, TotalAmount = 175_000m },
                    new TransactionLine { ItemName = "Gà rán", Type = TransactionLineType.ProductSale, Quantity = 3, UnitPrice = 25_000m, TotalAmount = 75_000m },
                ],
            },
            new Transaction
            {
                Code = "TXN006",
                AccountId = "ACC001",
                MemberId = "#0002",
                PaymentMethod = PaymentMethod.Banking,
                SubtotalAmount = 390_000m,
                DiscountAmount = 19_500m,
                TotalAmount = 370_500m,
                CreatedAt = new DateTime(2024, 3, 13, 11, 0, 0),
                Lines =
                [
                    new TransactionLine { ItemName = "Thuê bàn 05", Type = TransactionLineType.AreaRental, Quantity = 2, UnitPrice = 10_000m, TotalAmount = 20_000m },
                    new TransactionLine { ItemName = "Azul", Type = TransactionLineType.BoardGameRental, Quantity = 2, UnitPrice = 25_000m, TotalAmount = 50_000m },
                    new TransactionLine { ItemName = "Sinh tố bơ", Type = TransactionLineType.ProductSale, Quantity = 4, UnitPrice = 40_000m, TotalAmount = 160_000m },
                    new TransactionLine { ItemName = "Mì Ý", Type = TransactionLineType.ProductSale, Quantity = 2, UnitPrice = 80_000m, TotalAmount = 160_000m },
                ],
            },
            new Transaction
            {
                Code = "TXN007",
                AccountId = "ACC001",
                MemberId = "#0005",
                PaymentMethod = PaymentMethod.Cash,
                SubtotalAmount = 125_000m,
                DiscountAmount = 0m,
                TotalAmount = 125_000m,
                CreatedAt = new DateTime(2024, 3, 12, 20, 15, 0),
                Lines =
                [
                    new TransactionLine { ItemName = "Thuê bàn 01", Type = TransactionLineType.AreaRental, Quantity = 1, UnitPrice = 10_000m, TotalAmount = 10_000m },
                    new TransactionLine { ItemName = "Splendor", Type = TransactionLineType.BoardGameRental, Quantity = 1, UnitPrice = 15_000m, TotalAmount = 15_000m },
                    new TransactionLine { ItemName = "Cà phê sữa", Type = TransactionLineType.ProductSale, Quantity = 2, UnitPrice = 30_000m, TotalAmount = 60_000m },
                    new TransactionLine { ItemName = "Bánh mì", Type = TransactionLineType.ProductSale, Quantity = 2, UnitPrice = 20_000m, TotalAmount = 40_000m },
                ],
            },
            new Transaction
            {
                Code = "TXN008",
                AccountId = "ACC001",
                MemberId = "#0003",
                PaymentMethod = PaymentMethod.Banking,
                SubtotalAmount = 680_000m,
                DiscountAmount = 34_000m,
                TotalAmount = 646_000m,
                CreatedAt = new DateTime(2024, 3, 12, 13, 45, 0),
                Lines =
                [
                    new TransactionLine { ItemName = "Thuê phòng VIP", Type = TransactionLineType.AreaRental, Quantity = 4, UnitPrice = 80_000m, TotalAmount = 320_000m },
                    new TransactionLine { ItemName = "7 Wonders", Type = TransactionLineType.BoardGameRental, Quantity = 4, UnitPrice = 25_000m, TotalAmount = 100_000m },
                    new TransactionLine { ItemName = "Nước ép dưa hấu", Type = TransactionLineType.ProductSale, Quantity = 6, UnitPrice = 28_000m, TotalAmount = 168_000m },
                    new TransactionLine { ItemName = "Gà viên chiên", Type = TransactionLineType.ProductSale, Quantity = 2, UnitPrice = 46_000m, TotalAmount = 92_000m },
                ],
            },
        ];
    }
}
