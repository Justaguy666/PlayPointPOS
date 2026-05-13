using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Services;

namespace Infrastructure.Services.Media;

public sealed class HttpImageUploadService : IImageUploadService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient;
    private readonly IConfigurationService _configurationService;

    public HttpImageUploadService(HttpClient httpClient, IConfigurationService configurationService)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);

        string uploadUrl = BuildUploadEndpoint();
        using var content = new MultipartFormDataContent();
        var part = new StreamContent(fileStream);
        part.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(part, "file", string.IsNullOrWhiteSpace(fileName) ? "image.bin" : fileName);

        using HttpResponseMessage response = await _httpClient.PostAsync(uploadUrl, content, cancellationToken).ConfigureAwait(false);
        string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                string.Format(CultureInfo.InvariantCulture, "Image upload failed ({0}): {1}", (int)response.StatusCode, body));
        }

        UploadResponse? parsed = JsonSerializer.Deserialize<UploadResponse>(body, JsonOptions);
        if (parsed is null || string.IsNullOrWhiteSpace(parsed.Url))
        {
            throw new InvalidOperationException("Image upload returned no URL.");
        }

        return parsed.Url.Trim();
    }

    private string BuildUploadEndpoint()
    {
        string serverAddress = _configurationService.ServerAddress?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(serverAddress))
        {
            serverAddress = "http://localhost";
        }

        if (!serverAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            && !serverAddress.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            serverAddress = $"http://{serverAddress}";
        }

        int port = _configurationService.Port is >= 1 and <= 65535
            ? _configurationService.Port
            : 4000;

        return $"{serverAddress.TrimEnd('/')}:{port}/upload/image";
    }

    private sealed class UploadResponse
    {
        [JsonPropertyName("url")]
        public string? Url { get; init; }
    }
}
