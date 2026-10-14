using OnlyShop.Application.Abstractions.Infrastructure;

namespace OnlyShop.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string hash, string password) => BCrypt.Net.BCrypt.Verify(password, hash);
}