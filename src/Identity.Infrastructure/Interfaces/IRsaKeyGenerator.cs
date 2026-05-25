using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Interfaces;

internal interface IRsaKeyGenerator
{
    RsaSecurityKey GenerateKey(int keySize = 2048, string? keyId = null);
}
