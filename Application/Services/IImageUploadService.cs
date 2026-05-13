namespace Application.Services;

/// <summary>
/// Uploads a binary image to the backend and returns a public HTTPS URL (e.g. Cloudinary).
/// </summary>
public interface IImageUploadService
{
    /// <summary>
    /// Uploads image bytes. <paramref name="fileName"/> is used as the multipart filename hint.
    /// </summary>
    Task<string> UploadImageAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
}
