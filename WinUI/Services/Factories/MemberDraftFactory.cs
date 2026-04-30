using System;
using System.Collections.Generic;
using System.Linq;
using Application.Members;
using Application.Services.Members;
using WinUI.UIModels.Management;

namespace WinUI.Services.Factories;

public sealed class MemberDraftFactory
{
    private readonly IMembershipRankManagementService _rankManagementService;

    public MemberDraftFactory(IMembershipRankManagementService rankManagementService)
    {
        _rankManagementService = rankManagementService ?? throw new ArgumentNullException(nameof(rankManagementService));
    }

    public MemberModel Create(IEnumerable<MemberModel> existingMembers, IReadOnlyList<MembershipRank> ranks)
    {
        var member = new MemberModel
        {
            Code = GenerateNextMemberCode(existingMembers),
            FullName = string.Empty,
            PhoneNumber = string.Empty,
            TotalSpentAmount = 0m,
        };

        ApplyMembershipState(member, ranks);
        return member;
    }

    public void NormalizeRanks(IList<MembershipRank> ranks)
    {
        _rankManagementService.NormalizeRanks(ranks);
    }

    public void ApplyMembershipState(MemberModel member, IReadOnlyList<MembershipRank> ranks)
    {
        _rankManagementService.ApplyMembershipState(member, ranks);
    }

    public void RefreshMembershipStates(IEnumerable<MemberModel> members, IReadOnlyList<MembershipRank> ranks)
    {
        _rankManagementService.RefreshMembershipStates(members, ranks);
    }

    private static string GenerateNextMemberCode(IEnumerable<MemberModel> existingMembers)
    {
        int currentMaxValue = existingMembers
            .Select(member =>
            {
                string digits = new(member.Code.Where(char.IsDigit).ToArray());
                return int.TryParse(digits, out int parsedValue) ? parsedValue : 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        return $"#{currentMaxValue + 1:0000}";
    }
}
