using System;
using System.Threading.Tasks;
using Application.Services;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace WinUI.Services;

public sealed class WinUIFilePickerService : IFilePickerService
{
    private readonly MainWindow _mainWindow;

    public WinUIFilePickerService(MainWindow mainWindow)
    {
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
    }

    public async Task<string?> PickImageFileUriAsync()
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".webp");
        picker.FileTypeFilter.Add(".bmp");

        nint windowHandle = WindowNative.GetWindowHandle(_mainWindow);
        InitializeWithWindow.Initialize(picker, windowHandle);

        var file = await picker.PickSingleFileAsync();
        return file is null ? null : new Uri(file.Path).AbsoluteUri;
    }
}
