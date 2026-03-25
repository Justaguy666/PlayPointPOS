using Application.Services;

namespace Infrastructure.Services;

public class AppInfoService : IAppInfoService
{
    public string GetAppVersion()
    {
        var v = Windows.ApplicationModel.Package.Current.Id.Version;
        return $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
    }
}
