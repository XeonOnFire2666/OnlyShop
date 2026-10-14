using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;

namespace OnlyShop.Adapters.Telegram;

internal sealed class StaffLinkCodes(IMemoryCache cache)
{
    private const string Prefix = "staff-link:";

    public string Create(Guid staffId)
    {
        var code = Convert.ToHexString(RandomNumberGenerator.GetBytes(8)).ToLowerInvariant();
        cache.Set(Prefix + code, staffId, TimeSpan.FromMinutes(10));
        return code;
    }

    public bool TryConsume(string code, out Guid staffId)
    {
        if (cache.TryGetValue(Prefix + code, out Guid id))
        {
            cache.Remove(Prefix + code);
            staffId = id;
            return true;
        }

        staffId = default;
        return false;
    }
}
