namespace Identity.Infrastructure.Interfaces;

/// <summary>
/// A freshly generated RSA key pair, split into the public JWK members and the PKCS#8 private key.
/// </summary>
/// <param name="Kid">RFC 7638 JWK thumbprint of the public key.</param>
/// <param name="N">Base64url modulus.</param>
/// <param name="E">Base64url public exponent.</param>
/// <param name="Pkcs8PrivateKey">Unencrypted PKCS#8 private key. Protect and zero it promptly.</param>
internal sealed record GeneratedRsaKey(string Kid, string N, string E, byte[] Pkcs8PrivateKey);

internal interface IRsaKeyGenerator
{
    GeneratedRsaKey GenerateKey(int keySize = 2048);
}
