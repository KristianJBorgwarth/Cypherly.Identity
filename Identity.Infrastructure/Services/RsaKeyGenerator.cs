using System.Security.Cryptography;
using Identity.Infrastructure.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Services;

internal sealed class RsaKeyGenerator : IRsaKeyGenerator
{
    public RsaSecurityKey GenerateKey(int keySize = 2048, string? keyId = null)
    {
        using RSA rsa = RSA.Create();
        rsa.KeySize = keySize;
        RSAParameters parameters = rsa.ExportParameters(true);
        return new RsaSecurityKey(parameters) { KeyId = string.IsNullOrWhiteSpace(keyId) ? Guid.NewGuid().ToString() : keyId };
    }
}
