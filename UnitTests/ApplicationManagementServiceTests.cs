using Application.Areas;
using Application.Games;
using Application.Members;
using Application.Transactions;
using Domain.Enums;

namespace UnitTests;

public class ApplicationManagementServiceTests
{
    [Fact]
    public void GameTypeManagementService_Add_TrimsAndDeduplicatesByName()
    {
        var service = new GameTypeManagementService();
        var gameTypes = new List<GameType>
        {
            new() { Name = "Strategy" },
        };

        GameType result = service.Add(gameTypes, " strategy ");

        Assert.Single(gameTypes);
        Assert.Same(gameTypes[0], result);
        Assert.Equal("Strategy", result.Name);
    }

    [Fact]
    public void MembershipRankManagementService_NormalizesRanksAndAppliesProgress()
    {
        var service = new MembershipRankManagementService();
        var ranks = new List<MembershipRank>
        {
            new() { Name = "Gold", MinSpentAmount = 500m },
            new() { Name = "Bronze", MinSpentAmount = 0m },
            new() { Name = "Silver", MinSpentAmount = 100m },
        };
        var member = new TestMemberRankProgressState { TotalSpentAmount = 50m };

        service.NormalizeRanks(ranks);
        service.ApplyMembershipState(member, ranks);

        Assert.Equal(["Bronze", "Silver", "Gold"], ranks.Select(rank => rank.Name));
        Assert.Equal([1, 2, 3], ranks.Select(rank => rank.Priority));
        Assert.True(ranks[0].IsDefault);
        Assert.False(ranks[1].IsDefault);
        Assert.Same(ranks[0], member.MembershipRank);
        Assert.Same(ranks[1], member.NextMembershipRank);
        Assert.Equal(50, member.ProgressPercentage);
    }

    [Fact]
    public void AreaSessionService_StartSessionAndPaymentSummary_UseApplicationRules()
    {
        var service = new AreaSessionService();
        var startedAt = new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc);
        var area = new TestAreaSessionState
        {
            HourlyPrice = 100_000m,
            MaxCapacity = 4,
        };

        service.StartSession(
            area,
            new StartAreaSessionRequest
            {
                CustomerName = "  Linh  ",
                PhoneNumber = " 0909000000 ",
                Capacity = 99,
            },
            startedAt);
        AreaPaymentSummary summary = service.CalculatePaymentSummary(area, startedAt.AddMinutes(90));

        Assert.Equal(PlayAreaStatus.Rented, area.Status);
        Assert.Equal("Linh", area.CustomerName);
        Assert.Equal("0909000000", area.PhoneNumber);
        Assert.Equal(4, area.Capacity);
        Assert.Equal(TimeSpan.FromMinutes(90), summary.ElapsedTime);
        Assert.Equal(150_000m, summary.AreaFee);
        Assert.Equal(150_000m, summary.Total);
    }

    [Fact]
    public void TransactionFilterService_AppliesPaymentAmountAndDateFilters()
    {
        var service = new TransactionFilterService();
        var transactions = new[]
        {
            new TestTransaction(PaymentMethod.Cash, 150m, new DateTime(2026, 5, 1)),
            new TestTransaction(PaymentMethod.Banking, 150m, new DateTime(2026, 5, 1)),
            new TestTransaction(PaymentMethod.Cash, 50m, new DateTime(2026, 5, 1)),
            new TestTransaction(PaymentMethod.Cash, 150m, new DateTime(2026, 4, 1)),
        };

        IReadOnlyList<TestTransaction> result = service.Apply(
            transactions,
            new TransactionFilter
            {
                PaymentMethod = PaymentMethod.Cash,
                AmountMin = 100m,
                AmountMax = 200m,
                DateFrom = new DateTime(2026, 5, 1),
                DateTo = new DateTime(2026, 5, 2),
            });

        Assert.Single(result);
        Assert.Equal(transactions[0], result[0]);
    }

    private sealed class TestMemberRankProgressState : IMemberRankProgressState
    {
        public decimal TotalSpentAmount { get; init; }

        public MembershipRank? MembershipRank { get; set; }

        public MembershipRank? NextMembershipRank { get; set; }

        public int ProgressPercentage { get; set; }
    }

    private sealed class TestAreaSessionState : IAreaSessionState, IAreaCapacity
    {
        public decimal HourlyPrice { get; init; }

        public int MaxCapacity { get; init; }

        public string CustomerName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string? MemberId { get; set; }

        public DateTime? CheckInDateTime { get; set; }

        public int Capacity { get; set; }

        public DateTime? StartTime { get; set; }

        public bool IsSessionPaused { get; set; }

        public DateTime? SessionPausedAt { get; set; }

        public TimeSpan SessionPausedDuration { get; set; }

        public decimal TotalAmount { get; set; }

        public PlayAreaStatus Status { get; set; }
    }

    private sealed record TestTransaction(
        PaymentMethod PaymentMethod,
        decimal TotalAmount,
        DateTime CreatedAt) : ITransactionFilterable;
}
