namespace OnlyShop.Application.Abstractions.Infrastructure;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string hash, string password);
}