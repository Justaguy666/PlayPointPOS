using Application.Services;

namespace WinUI.Services;

public sealed class WinUIApplicationLifetimeService : IApplicationLifetimeService
{
    private bool _isExiting;

    public void Exit()
    {
        if (_isExiting)
        {
            return;
        }

        _isExiting = true;
        Microsoft.UI.Xaml.Application.Current.Exit();
    }
}
