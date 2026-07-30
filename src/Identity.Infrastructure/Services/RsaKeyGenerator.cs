using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Identity.Infrastructure.Interfaces;

namespace Identity.Infrastructure.Services;

internal sealed class RsaKeyGenerator : IRsaKeyGenerator
{
    public GeneratedRsaKey GenerateKey(int keySize = 2048)
    {
        using var rsa = RSA.Create(keySize);

        var parameters = rsa.ExportParameters(false);
        var n = Base64Url.EncodeToString(parameters.Modulus!);
        var e = Base64Url.EncodeToString(parameters.Exponent!);

        return new GeneratedRsaKey(ComputeThumbprint(n, e), n, e, rsa.ExportPkcs8PrivateKey());
    }

    /// <summary>
    /// RFC 7638 JWK thumbprint: SHA-256 over the required members only, lexicographically
    /// ordered, no whitespace. Derives the kid from the key itself rather than a random id.
    /// </summary>
    private static string ComputeThumbprint(string n, string e)
    {
        var canonical = $$"""{"e":"{{e}}","kty":"RSA","n":"{{n}}"}""";
        return Base64Url.EncodeToString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }
}
