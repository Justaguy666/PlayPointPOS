using System;
using System.Collections.Generic;
using System.Linq;
using Application.Members;
using WinUI.UIModels.Management;
using WinUI.ViewModels.Dialogs.Management;

namespace WinUI.Services.Factories;

public sealed class MemberDraftFactory
{
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
        var orderedRanks = ranks
            .OrderBy(rank => rank.MinSpentAmount)
            .ThenBy(rank => rank.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        ranks.Clear();
        for (int index = 0; index < orderedRanks.Count; index++)
        {
            MembershipRank rank = orderedRanks[index];
            rank.Priority = index + 1;
            rank.IsDefault = index == 0;
            if (string.IsNullOrWhiteSpace(rank.Color))
            {
                rank.Color = MembershipPackageDialogViewModel.ResolveRankColor(rank.Name, index);
            }

            ranks.Add(rank);
        }
    }

    public void ApplyMembershipState(MemberModel member, IReadOnlyList<MembershipRank> ranks)
    {
        MemberRankProgressSnapshot snapshot = MemberRankProgressCalculator.Calculate(member.TotalSpentAmount, ranks);

        member.MembershipRank = null;
        member.MembershipRank = snapshot.CurrentRank;

        member.NextMembershipRank = null;
        member.NextMembershipRank = snapshot.NextRank;

        member.ProgressPercentage = snapshot.ProgressPercentage;
    }

    public void RefreshMembershipStates(IEnumerable<MemberModel> members, IReadOnlyList<MembershipRank> ranks)
    {
        foreach (MemberModel member in members)
        {
            ApplyMembershipState(member, ranks);
        }
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
