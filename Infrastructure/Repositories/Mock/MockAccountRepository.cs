using Domain.Entities;
using Infrastructure.Services;

namespace Infrastructure.Repositories.Mock;

public class MockAccountRepository : MockRepository<Account>
{
    public MockAccountRepository()
    {
        // Pre-seed with test account
        // Password "1" is hashed using BCrypt work factor 12
        var passwordHasher = new PasswordHasher();
        var hashedPassword = passwordHasher.HashPassword("1");

        _items.AddRange(new[]
        {
            new Account
            {
                Id = "1",
                Email = "1",
                PasswordHash = hashedPassword,  // ✅ Now properly hashed
                Language = "en-US",
            },
        });
    }
}

