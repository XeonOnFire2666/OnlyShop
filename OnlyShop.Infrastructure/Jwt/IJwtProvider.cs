using OnlyShop.Application.Dtos;

namespace OnlyShop.Infrastructure.Jwt;

public interface IJwtProvider
{
    string GenerateToken(StaffDto staff);
}