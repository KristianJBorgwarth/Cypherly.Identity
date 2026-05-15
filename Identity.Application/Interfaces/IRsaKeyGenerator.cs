using Microsoft.IdentityModel.Tokens;

namespace Identity.Application.Interfaces;

internal interface IRsaKeyGenerator
{
    RsaSecurityKey GenerateKey(int keySize = 2048, string? keyId = null);
}
