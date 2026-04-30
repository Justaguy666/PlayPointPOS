using Application.Interfaces;
using Application.Members;
using Application.Services.Members;
using Domain.Entities;

namespace Infrastructure.Services.Members;

public sealed class RepositoryMemberLookupService : IMemberLookupService
{
    private readonly IRepository<Member> _memberRepository;

    public RepositoryMemberLookupService(IRepository<Member> memberRepository)
    {
        _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
    }

    public IReadOnlyList<MemberLookupItem> GetActiveMembers()
    {
        return _memberRepository.GetAllAsync().GetAwaiter().GetResult()
            .Where(member => member.IsActive)
            .OrderBy(member => member.FullName)
            .Select(member => new MemberLookupItem
            {
                Id = member.Id,
                FullName = member.FullName,
                PhoneNumber = member.PhoneNumber,
            })
            .ToList();
    }
}
