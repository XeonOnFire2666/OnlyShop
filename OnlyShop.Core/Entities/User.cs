using OnlyShop.Core.Common;

namespace OnlyShop.Core.Entities;

public class User : BaseEntity
{
    private User()
    {
    }

    public long TelegramId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Username { get; private set; }
    public string? Phone { get; private set; }
    public decimal BonusBalance { get; private set; }

    public static User Create(long telegramId, string name, string? username = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Name is required");
        }

        return new User
        {
            TelegramId = telegramId,
            Name = name.Trim(),
            Username = username
        };
    }

    public void UpdateContact(string name, string? username, string? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Name is required");
        }

        Name = name.Trim();
        Username = username;
        Phone = phone;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddBonuses(decimal amount)
    {
        if (amount <= 0)
        {
            throw new DomainException("Bonus amount must be positive");
        }

        BonusBalance += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SpendBonuses(decimal amount)
    {
        if (amount <= 0)
        {
            throw new DomainException("Bonus amount must be positive");
        }

        if (amount > BonusBalance)
        {
            throw new DomainException("Not enough bonuses");
        }

        BonusBalance -= amount;
        UpdatedAt = DateTime.UtcNow;
    }
}