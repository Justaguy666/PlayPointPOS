using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Services;
using Application.UseCases.Auth.Contracts;
using Domain.Entities;

namespace Application.UseCases.Auth;

/// <summary>
/// Use case for registering a new user account.
/// Encapsulates the business logic for signup: validating email uniqueness, hashing password, and creating account.
/// </summary>
public class RegisterUserUseCase
{
    private readonly IRepository<Account> _accountRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserUseCase(
        IRepository<Account> accountRepository,
        IPasswordHasher passwordHasher)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    /// <summary>
    /// Executes the registration use case.
    /// </summary>
    /// <param name="email">User's email (must be unique)</param>
    /// <param name="password">User's plain-text password (will be hashed for storage)</param>
    /// <param name="language">User's preferred language (e.g., "en-US", "vi-VN")</param>
    /// <returns>RegisterResult containing success status and message</returns>
    public async Task<RegisterResult> ExecuteAsync(string email, string password, string language = "en-US")
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(email))
        {
            return new RegisterResult
            {
                Success = false,
                Message = "Email is required"
            };
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return new RegisterResult
            {
                Success = false,
                Message = "Password is required"
            };
        }

        if (password.Length < 6)
        {
            return new RegisterResult
            {
                Success = false,
                Message = "Password must be at least 6 characters"
            };
        }

        // Check if email already exists (case-insensitive)
        var existingAccounts = await _accountRepository.GetAllAsync();
        var emailExists = existingAccounts.Any(a =>
            a.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        if (emailExists)
        {
            return new RegisterResult
            {
                Success = false,
                Message = "Email already registered"
            };
        }

        // Hash the password
        var hashedPassword = _passwordHasher.HashPassword(password);

        // Create new account
        var newAccount = new Account
        {
            Email = email,
            PasswordHash = hashedPassword,
            Language = string.IsNullOrWhiteSpace(language) ? "en-US" : language
        };

        // Save to repository
        await _accountRepository.AddAsync(newAccount);

        // Registration successful
        return new RegisterResult
        {
            Success = true,
            Account = newAccount,
            Message = "Registration successful"
        };
    }
}
