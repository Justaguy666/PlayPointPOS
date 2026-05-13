namespace Application.Services;

public interface IAuthStateService
{
    string? ShopId { get; set; }

    void Clear();
}
