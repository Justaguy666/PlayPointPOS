using Application.UseCases.Auth.Contracts;

namespace Application.Services;

public interface IAuthApiService
{
    Task<AuthOperationResult> SendRegistrationOtpAsync(string email);

    Task<LoginResult> LoginAsync(string email, string password);

    Task<RegisterResult> RegisterAsync(string email, string password, string otpCode);
}
