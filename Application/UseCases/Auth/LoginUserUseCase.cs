using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Services;
using Application.UseCases.Auth.Contracts;
using Domain.Entities;

namespace Application.UseCases.Auth;

/// <summary>
/// Use case for authenticating a user.
/// Encapsulates the business logic for login: finding account by email and verifying password.
/// </summary>
public class LoginUserUseCase
{
    private readonly IRepository<Account> _accountRepository;
    private readonly IPasswordHasher _passwordHasher;

    public LoginUserUseCase(
        IRepository<Account> accountRepository,
        IPasswordHasher passwordHasher)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    /// <summary>
    /// Executes the login use case.
    /// </summary>
    /// <param name="email">User's email (case-insensitive)</param>
    /// <param name="password">User's plain-text password</param>
    /// <returns>LoginResult containing success status, account (if successful), and message</returns>
    public async Task<LoginResult> ExecuteAsync(string email, string password)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(email))
        {
            return new LoginResult
            {
                Success = false,
                Message = "Email is required"
            };
        }  
            

        if (string.IsNullOrWhiteSpace(password))
        {
            return new LoginResult
            {
                Success = false,
                Message = "Password is required"
            };
        }

        // Find account by email (case-insensitive)
        var accounts = await _accountRepository.GetAllAsync();
        var account = accounts.FirstOrDefault(a =>
            a.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        if (account == null)
        {
            return new LoginResult
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        // Verify password using hasher
        var passwordMatches = _passwordHasher.VerifyPassword(password, account.PasswordHash);
        if (!passwordMatches)
        {
            return new LoginResult
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        // Login successful
        return new LoginResult
        {
            Success = true,
            Account = account,
            Message = "Login successful"
        };
    }
}
