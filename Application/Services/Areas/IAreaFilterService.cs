using Application.Areas;

namespace Application.Services.Areas;

public interface IAreaFilterService
{
    IReadOnlyList<TArea> Apply<TArea>(IEnumerable<TArea> areas, PlayAreaFilter filter, string timeZone)
        where TArea : IAreaFilterable;
}
