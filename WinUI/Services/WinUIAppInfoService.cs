using Application.Services;

namespace WinUI.Services;

/// <summary>
/// WinUI implementation of app info service.
/// Uses Windows.ApplicationModel.Package (platform-specific).
/// </summary>
public class WinUIAppInfoService : IAppInfoService
{
    public string GetAppVersion()
    {
        try
        {
            var version = Windows.ApplicationModel.Package.Current.Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
        catch
        {
            return "0.0.0.0";
        }
    }
}
