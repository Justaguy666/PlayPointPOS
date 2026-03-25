namespace Infrastructure.Services.Navigation;

public static class NavigationMap
{
    public static readonly Dictionary<Type, Type> Map = new();

    public static void Register<TRequest, TPage>()
    {
        Map[typeof(TRequest)] = typeof(TPage);
    }
}
