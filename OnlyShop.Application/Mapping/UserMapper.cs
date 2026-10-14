using OnlyShop.Application.Dtos;
using OnlyShop.Core.Entities;

namespace OnlyShop.Application.Mapping;

internal static class UserMapper
{
    public static UserDto ToDto(this User user) =>
        new(user.Id, user.TelegramId, user.Name, user.Username, user.Phone, user.BonusBalance);

    public static StaffDto ToDto(this StaffMember staff) =>
        new(staff.Id, staff.Email, staff.Name, staff.Role, staff.IsActive, staff.TelegramId);

    public static CategoryDto ToDto(this Category category) =>
        new(category.Id, category.Name, category.Slug, category.ParentId, category.SortOrder);
}