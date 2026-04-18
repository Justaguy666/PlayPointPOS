using Application.Areas;

namespace Application.Services.Areas;

public interface IAreaCatalogService
{
    IReadOnlyList<AreaRecord> GetAreas();
}
