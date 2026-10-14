using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using OnlyShop.Adapters.Common;
using OnlyShop.Application.Abstractions.Services;
using OnlyShop.Application.Common.Exceptions;
using OnlyShop.Infrastructure.Jwt;

namespace OnlyShop.Adapters.Endpoints;

internal static class AuthEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/auth/login", async (
            StaffLoginRequest request,
            IStaffService staff,
            IJwtProvider jwt,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                throw new UnauthorizedException("Invalid email or password");
            }

            var member = await staff.AuthenticateAsync(request.Email, request.Password, ct);
            return new StaffAuthResponse(jwt.GenerateToken(member), member);
        }).AllowAnonymous().RequireRateLimiting("auth");
    }
}