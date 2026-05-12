using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Services;
using Application.UseCases.Auth.Contracts;
using Domain.Entities;

namespace Infrastructure.Services.Auth;

public sealed class GraphQLAuthApiService : IAuthApiService
{
    private const string SendOtpMutation = """
        mutation SendOtp($input: SendOtpInput!) {
          sendOtp(input: $input) {
            code
            success
            message
          }
        }
        """;

    private const string LoginMutation = """
        mutation Login($input: LoginShopInput!) {
          login(input: $input) {
            code
            success
            message
            shopId
            accessToken
            refreshToken
          }
        }
        """;

    private const string RegisterMutation = """
        mutation Register($input: RegisterShopInput!) {
          register(input: $input) {
            code
            success
            message
            shopId
            accessToken
            refreshToken
          }
        }
        """;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient;
    private readonly IConfigurationService _configurationService;

    public GraphQLAuthApiService(HttpClient httpClient, IConfigurationService configurationService)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
    }

    public async Task<AuthOperationResult> SendRegistrationOtpAsync(string email)
    {
        GraphQLResponse<MutationEnvelope>? response = await PostAsync<MutationEnvelope>(
            SendOtpMutation,
            new
            {
                input = new
                {
                    email,
                },
            });

        if (response?.Errors is { Length: > 0 })
        {
            return new AuthOperationResult
            {
                Success = false,
                Message = response.Errors[0].Message,
            };
        }

        MutationPayload? payload = response?.Data?.SendOtp;
        return new AuthOperationResult
        {
            Success = payload?.Success == true,
            Message = payload?.Message ?? "Unable to send OTP.",
        };
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        GraphQLResponse<LoginEnvelope>? response = await PostAsync<LoginEnvelope>(
            LoginMutation,
            new
            {
                input = new
                {
                    email,
                    password,
                },
            });

        if (response?.Errors is { Length: > 0 })
        {
            return new LoginResult
            {
                Success = false,
                Message = response.Errors[0].Message,
            };
        }

        ShopMutationPayload? payload = response?.Data?.Login;
        if (payload?.Success != true)
        {
            return new LoginResult
            {
                Success = false,
                Message = payload?.Message ?? "Login failed.",
            };
        }

        return new LoginResult
        {
            Success = true,
            Message = payload.Message ?? "Login successful.",
            Account = new Account
            {
                Id = payload.GetShopId(),
                Email = email,
                ShopName = string.Empty,
            },
        };
    }

    public async Task<RegisterResult> RegisterAsync(string email, string password, string otpCode)
    {
        GraphQLResponse<RegisterEnvelope>? response = await PostAsync<RegisterEnvelope>(
            RegisterMutation,
            new
            {
                input = new
                {
                    email,
                    password,
                    otp = otpCode,
                },
            });

        if (response?.Errors is { Length: > 0 })
        {
            return new RegisterResult
            {
                Success = false,
                Message = response.Errors[0].Message,
            };
        }

        ShopMutationPayload? payload = response?.Data?.Register;
        if (payload?.Success != true)
        {
            return new RegisterResult
            {
                Success = false,
                Message = payload?.Message ?? "Registration failed.",
            };
        }

        return new RegisterResult
        {
            Success = true,
            Message = payload.Message ?? "Registration successful.",
            Account = new Account
            {
                Id = payload.GetShopId(),
                Email = email,
                ShopName = string.Empty,
            },
        };
    }

    private async Task<GraphQLResponse<TData>?> PostAsync<TData>(string query, object variables)
    {
        string endpoint = BuildEndpoint();
        string json = JsonSerializer.Serialize(new GraphQLRequest
        {
            Query = query,
            Variables = variables,
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };

        using HttpResponseMessage response = await _httpClient.SendAsync(request);
        string responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(string.Format(
                CultureInfo.InvariantCulture,
                "Auth API request failed with status {0}: {1}",
                (int)response.StatusCode,
                responseContent));
        }

        return JsonSerializer.Deserialize<GraphQLResponse<TData>>(responseContent, JsonOptions);
    }

    private string BuildEndpoint()
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

        return $"{serverAddress.TrimEnd('/')}:{port}/";
    }

    private sealed class GraphQLRequest
    {
        [JsonPropertyName("query")]
        public string Query { get; init; } = string.Empty;

        [JsonPropertyName("variables")]
        public object Variables { get; init; } = new();
    }

    private sealed class GraphQLResponse<TData>
    {
        [JsonPropertyName("data")]
        public TData? Data { get; init; }

        [JsonPropertyName("errors")]
        public GraphQLError[]? Errors { get; init; }
    }

    private sealed class GraphQLError
    {
        [JsonPropertyName("message")]
        public string Message { get; init; } = string.Empty;
    }

    private sealed class MutationEnvelope
    {
        [JsonPropertyName("sendOtp")]
        public MutationPayload? SendOtp { get; init; }
    }

    private sealed class LoginEnvelope
    {
        [JsonPropertyName("login")]
        public ShopMutationPayload? Login { get; init; }
    }

    private sealed class RegisterEnvelope
    {
        [JsonPropertyName("register")]
        public ShopMutationPayload? Register { get; init; }
    }

    private class MutationPayload
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("message")]
        public string? Message { get; init; }
    }

    private sealed class ShopMutationPayload : MutationPayload
    {
        [JsonPropertyName("shopId")]
        public JsonElement ShopId { get; init; }

        public string GetShopId()
        {
            return ShopId.ValueKind switch
            {
                JsonValueKind.Number => ShopId.GetInt32().ToString(CultureInfo.InvariantCulture),
                JsonValueKind.String => ShopId.GetString() ?? string.Empty,
                _ => string.Empty,
            };
        }
    }
}
