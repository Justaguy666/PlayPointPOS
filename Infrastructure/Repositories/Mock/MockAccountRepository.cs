using Domain.Entities;

namespace Infrastructure.Repositories.Mock;

public class MockAccountRepository : MockRepository<Account>
{
    public MockAccountRepository()
    {
        _items.AddRange(new[]
        {
            new Account
            {
                Id = "1",
                Email = "1",
                PasswordHash = "1",
                Language = "en-US",
            },
        });
    }
}
