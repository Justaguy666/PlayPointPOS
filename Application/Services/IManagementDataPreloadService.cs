namespace Application.Services;

public interface IManagementDataPreloadService
{
    Task WarmUpAsync();

    void Clear();
}
