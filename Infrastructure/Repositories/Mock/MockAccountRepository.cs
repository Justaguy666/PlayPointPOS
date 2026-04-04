using Domain.Entities;
using Infrastructure.Services;

namespace Infrastructure.Repositories.Mock;

public class MockAccountRepository : MockRepository<Account>
{
    public MockAccountRepository()
    {
        var passwordHasher = new PasswordHasher();
        var hashedPassword = passwordHasher.HashPassword("1");

        _items.AddRange(new[]
        {
            new Account
            {
                Id = "1",
                Email = "1",
                PasswordHash = hashedPassword,
            },
        });
    }
}
