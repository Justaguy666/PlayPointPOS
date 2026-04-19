using Application.Members;

namespace Application.Services.Members;

public interface IMemberCatalogService
{
    IReadOnlyList<MemberRecord> GetMembers();
}
