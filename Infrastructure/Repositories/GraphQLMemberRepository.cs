using System.Linq;
using Application.Interfaces;
using Application.Members;
using Application.Services.Members;
using Domain.Entities;

namespace Infrastructure.Repositories;

/// <summary>
/// Read-only member list backed by the management GraphQL API (replaces mock for reservation / start session flows).
/// </summary>
public sealed class GraphQLMemberRepository : IRepository<Member>
{
    private readonly IMemberCatalogService _memberCatalogService;

    public GraphQLMemberRepository(IMemberCatalogService memberCatalogService)
    {
        _memberCatalogService = memberCatalogService ?? throw new ArgumentNullException(nameof(memberCatalogService));
    }

    public Task<IEnumerable<Member>> GetAllAsync()
    {
        IReadOnlyList<MemberRecord> records = _memberCatalogService.GetMembers();
        IEnumerable<Member> members = records.Select(ToMember);
        return Task.FromResult(members);
    }

    public Task<Member?> GetByIdAsync(string id)
    {
        MemberRecord? record = _memberCatalogService.GetMembers()
            .FirstOrDefault(m => string.Equals(m.Id, id, StringComparison.Ordinal));
        return Task.FromResult(record is null ? null : ToMember(record));
    }

    public Task AddAsync(Member entity) =>
        throw new NotSupportedException($"{nameof(GraphQLMemberRepository)} is read-only.");

    public Task UpdateAsync(Member entity) =>
        throw new NotSupportedException($"{nameof(GraphQLMemberRepository)} is read-only.");

    public Task DeleteAsync(string id) =>
        throw new NotSupportedException($"{nameof(GraphQLMemberRepository)} is read-only.");

    private static Member ToMember(MemberRecord record)
    {
        return new Member
        {
            Id = record.Id ?? string.Empty,
            FullName = record.FullName ?? string.Empty,
            PhoneNumber = record.PhoneNumber ?? string.Empty,
            MembershipId = record.CurrentRank?.Id ?? string.Empty,
            IsActive = true,
        };
    }
}
