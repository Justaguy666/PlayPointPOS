namespace Application.Services;

public interface IFilePickerService
{
    Task<string?> PickImageFileUriAsync();
}
