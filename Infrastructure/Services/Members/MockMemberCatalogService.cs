using Application.Members;
using Application.Services.Members;
using Domain.Entities;

namespace Infrastructure.Services.Members;

public sealed class MockMemberCatalogService : IMemberCatalogService
{
    private readonly IMembershipRankCatalogService _membershipRankCatalogService;

    public MockMemberCatalogService(IMembershipRankCatalogService membershipRankCatalogService)
    {
        _membershipRankCatalogService = membershipRankCatalogService ?? throw new ArgumentNullException(nameof(membershipRankCatalogService));
    }

    public IReadOnlyList<MemberRecord> GetMembers()
    {
        IReadOnlyList<MembershipRank> membershipRanks = _membershipRankCatalogService.GetMembershipRanks();

        MembershipRank bronzeRank = membershipRanks.First(rank => rank.Name == "Bronze");
        MembershipRank silverRank = membershipRanks.First(rank => rank.Name == "Silver");
        MembershipRank goldRank = membershipRanks.First(rank => rank.Name == "Gold");
        MembershipRank platinumRank = membershipRanks.First(rank => rank.Name == "Platinum");
        MembershipRank diamondRank = membershipRanks.First(rank => rank.Name == "Diamond");

        return
        [
            new MemberRecord { Code = "#0001", FullName = "Nguyen Van A", PhoneNumber = "0901234567", TotalSpentAmount = 45_600_000m, CurrentRank = diamondRank },
            new MemberRecord { Code = "#0002", FullName = "Tran Minh Khoa", PhoneNumber = "0912345678", TotalSpentAmount = 24_500_000m, CurrentRank = platinumRank },
            new MemberRecord { Code = "#0003", FullName = "Le Thu Ha", PhoneNumber = "0987654321", TotalSpentAmount = 10_800_000m, CurrentRank = goldRank },
            new MemberRecord { Code = "#0004", FullName = "Pham Gia Bao", PhoneNumber = "0934567890", TotalSpentAmount = 4_200_000m, CurrentRank = silverRank },
            new MemberRecord { Code = "#0005", FullName = "Vo Ngoc Linh", PhoneNumber = "0978123456", TotalSpentAmount = 2_350_000m, CurrentRank = bronzeRank },
            new MemberRecord { Code = "#0006", FullName = "Nguyen Huy Hoang", PhoneNumber = "0908765432", TotalSpentAmount = 6_700_000m, CurrentRank = silverRank },
            new MemberRecord { Code = "#0007", FullName = "Pham Khanh An", PhoneNumber = "0945678123", TotalSpentAmount = 12_900_000m, CurrentRank = goldRank },
            new MemberRecord { Code = "#0008", FullName = "Do Mai Anh", PhoneNumber = "0923456789", TotalSpentAmount = 38_400_000m, CurrentRank = diamondRank },
            new MemberRecord { Code = "#0009", FullName = "Bui Tuan Kiet", PhoneNumber = "0967891234", TotalSpentAmount = 20_300_000m, CurrentRank = platinumRank },
        ];
    }
}
