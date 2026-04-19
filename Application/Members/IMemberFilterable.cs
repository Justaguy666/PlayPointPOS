using Domain.Entities;

namespace Application.Members;

public interface IMemberFilterable
{
    MembershipRank? MembershipRank { get; }

    decimal TotalSpentAmount { get; }
}
