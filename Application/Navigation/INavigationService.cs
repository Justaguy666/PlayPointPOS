namespace Application.Navigation;

public interface INavigationService
{
    void SetFrame(object frame);
    void Navigate(INavigationRequest request);
}
