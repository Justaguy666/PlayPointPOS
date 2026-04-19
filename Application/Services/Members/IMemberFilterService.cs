using Application.Members;

namespace Application.Services.Members;

public interface IMemberFilterService
{
    IReadOnlyList<TMember> Apply<TMember>(IEnumerable<TMember> members, MemberFilter filter)
        where TMember : IMemberFilterable;
}
