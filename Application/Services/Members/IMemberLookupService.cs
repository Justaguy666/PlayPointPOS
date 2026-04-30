using Application.Members;

namespace Application.Services.Members;

public interface IMemberLookupService
{
    IReadOnlyList<MemberLookupItem> GetActiveMembers();
}
