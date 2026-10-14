using OnlyShop.Core.Common;
using OnlyShop.Core.Enums;

namespace OnlyShop.Core.Entities;

public class StaffMember : BaseEntity
{
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string Name { get; set; }
    public StaffRole Role { get; set; } = StaffRole.Manager;
    public bool IsActive { get; set; } = true;
    public long? TelegramId { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public bool IsAdmin => Role == StaffRole.Admin;

    public void RegisterLogin() => LastLoginAt = DateTime.UtcNow;
}