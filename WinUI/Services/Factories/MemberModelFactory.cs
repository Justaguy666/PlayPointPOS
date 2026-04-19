using Application.Members;
using System;
using WinUI.UIModels.Management;

namespace WinUI.Services.Factories;

public sealed class MemberModelFactory
{
    public MemberModel Create(MemberRecord source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new MemberModel
        {
            Code = source.Code,
            FullName = source.FullName,
            PhoneNumber = source.PhoneNumber,
            TotalSpentAmount = source.TotalSpentAmount,
            MembershipRank = source.CurrentRank,
            ProgressPercentage = 0,
        };
    }

    public MemberModel Clone(MemberModel source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new MemberModel
        {
            Code = source.Code,
            FullName = source.FullName,
            PhoneNumber = source.PhoneNumber,
            TotalSpentAmount = source.TotalSpentAmount,
            MembershipRank = source.MembershipRank,
            NextMembershipRank = source.NextMembershipRank,
            ProgressPercentage = source.ProgressPercentage,
        };
    }
}
