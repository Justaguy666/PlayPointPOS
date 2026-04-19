using Application.Services.Members;
using Domain.Entities;

namespace Infrastructure.Services.Members;

public sealed class MockMembershipRankCatalogService : IMembershipRankCatalogService
{
    public IReadOnlyList<MembershipRank> GetMembershipRanks()
    {
        return
        [
            new MembershipRank
            {
                Name = "Bronze",
                Priority = 1,
                Color = "#F09A44",
                DiscountRate = 0.02m,
                MinSpentAmount = 0m,
                IsDefault = true,
            },
            new MembershipRank
            {
                Name = "Silver",
                Priority = 2,
                Color = "#CBD3DF",
                DiscountRate = 0.04m,
                MinSpentAmount = 3_000_000m,
                IsDefault = false,
            },
            new MembershipRank
            {
                Name = "Gold",
                Priority = 3,
                Color = "#F9CC45",
                DiscountRate = 0.06m,
                MinSpentAmount = 8_000_000m,
                IsDefault = false,
            },
            new MembershipRank
            {
                Name = "Platinum",
                Priority = 4,
                Color = "#72C5E4",
                DiscountRate = 0.08m,
                MinSpentAmount = 18_000_000m,
                IsDefault = false,
            },
            new MembershipRank
            {
                Name = "Diamond",
                Priority = 5,
                Color = "#9B8BFF",
                DiscountRate = 0.10m,
                MinSpentAmount = 35_000_000m,
                IsDefault = false,
            },
        ];
    }
}
