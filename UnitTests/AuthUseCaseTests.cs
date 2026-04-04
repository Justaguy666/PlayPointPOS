using Application.Interfaces;
using Application.UseCases.Auth;
using Domain.Entities;
using Infrastructure.Services;

namespace UnitTests;

public class AuthUseCaseTests
{
    [Fact]
    public async Task LoginUserUseCase_ReturnsSuccess_ForValidCredentials()
    {
        var passwordHasher = new PasswordHasher();
        var repository = new InMemoryAccountRepository(
            new Account
            {
                Id = "acc-1",
                Email = "owner@playpoint.test",
                PasswordHash = passwordHasher.HashPassword("secret-123"),
            });

        var useCase = new LoginUserUseCase(repository, passwordHasher);

        var result = await useCase.ExecuteAsync("OWNER@playpoint.test", "secret-123");

        Assert.True(result.Success);
        Assert.NotNull(result.Account);
        Assert.Equal("owner@playpoint.test", result.Account!.Email);
    }

    [Fact]
    public async Task RegisterUserUseCase_RejectsDuplicateEmail_IgnoringCase()
    {
        var passwordHasher = new PasswordHasher();
        var repository = new InMemoryAccountRepository(
            new Account
            {
                Id = "acc-1",
                Email = "owner@playpoint.test",
                PasswordHash = passwordHasher.HashPassword("secret-123"),
            });

        var useCase = new RegisterUserUseCase(repository, passwordHasher);

        var result = await useCase.ExecuteAsync("OWNER@playpoint.test", "another-secret");

        Assert.False(result.Success);
        Assert.Equal("Email already registered", result.Message);
    }

    [Fact]
    public async Task RegisterUserUseCase_HashesPassword_AndPersistsAccount()
    {
        var passwordHasher = new PasswordHasher();
        var repository = new InMemoryAccountRepository();
        var useCase = new RegisterUserUseCase(repository, passwordHasher);

        var result = await useCase.ExecuteAsync("new@playpoint.test", "secret-123");

        Assert.True(result.Success);
        Assert.NotNull(result.Account);
        Assert.NotEqual("secret-123", result.Account!.PasswordHash);
        Assert.True(passwordHasher.VerifyPassword("secret-123", result.Account.PasswordHash));
        Assert.Contains(await repository.GetAllAsync(), account => account.Email == "new@playpoint.test");
    }

    private sealed class InMemoryAccountRepository : IRepository<Account>
    {
        private readonly List<Account> _accounts;

        public InMemoryAccountRepository(params Account[] accounts)
        {
            _accounts = accounts.ToList();
        }

        public Task<IEnumerable<Account>> GetAllAsync()
            => Task.FromResult<IEnumerable<Account>>(_accounts.ToList());

        public Task<Account?> GetByIdAsync(string id)
            => Task.FromResult(_accounts.FirstOrDefault(account => account.Id == id));

        public Task AddAsync(Account entity)
        {
            _accounts.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Account entity)
        {
            int index = _accounts.FindIndex(account => account.Id == entity.Id);
            if (index >= 0)
            {
                _accounts[index] = entity;
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(string id)
        {
            _accounts.RemoveAll(account => account.Id == id);
            return Task.CompletedTask;
        }
    }
}
