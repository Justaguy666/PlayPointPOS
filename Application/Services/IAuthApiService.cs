using Application.UseCases.Auth.Contracts;

namespace Application.Services;

public interface IAuthApiService
{
    Task<AuthOperationResult> SendRegistrationOtpAsync(string email);

    Task<AuthOperationResult> SendPasswordResetOtpAsync(string email);

    Task<AuthOperationResult> ResetPasswordAsync(string email, string newPassword, string otpCode);

    Task<LoginResult> LoginAsync(string email, string password);

    Task<RegisterResult> RegisterAsync(string email, string password, string otpCode);
}
