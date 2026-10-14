using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OnlyShop.Adapters.Common;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetId(this ClaimsPrincipal user)
    {
        var raw = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(raw, out var id) && id != Guid.Empty)
        {
            return id;
        }

        throw new UnauthorizedAccessException("User id claim 'sub' is missing or invalid.");
    }
}