using System.Security.Cryptography;
using System.Text.Json;

namespace Identity.Test.Unit.Setup.Fixtures;

public sealed class PrivacyPassFixture
{
    private static readonly Lazy<PrivacyPassFixture> Instance = new(Load);

    public static PrivacyPassFixture Values => Instance.Value;

    public required PrivateKeyJwk PrivateKeyJwk { get; init; }
    public required string PublicKeyPssSpki { get; init; }
    public required string TokenRequest { get; init; }
    public required string TokenResponse { get; init; }

    public byte[] IssuerPrivateKeyPkcs8()
    {
        using var rsa = RSA.Create();
        rsa.ImportParameters(new RSAParameters
        {
            Modulus = Base64Url(PrivateKeyJwk.N),
            Exponent = Base64Url(PrivateKeyJwk.E),
            D = Base64Url(PrivateKeyJwk.D),
            P = Base64Url(PrivateKeyJwk.P),
            Q = Base64Url(PrivateKeyJwk.Q),
            DP = Base64Url(PrivateKeyJwk.Dp),
            DQ = Base64Url(PrivateKeyJwk.Dq),
            InverseQ = Base64Url(PrivateKeyJwk.Qi),
        });
        return rsa.ExportPkcs8PrivateKey();
    }

    public byte[] IssuerPublicKey() => Convert.FromBase64String(PublicKeyPssSpki);

    private static byte[] Base64Url(string value) => System.Buffers.Text.Base64Url.DecodeFromChars(value);

    private static PrivacyPassFixture Load() =>
        JsonSerializer.Deserialize<PrivacyPassFixture>(
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Setup", "Fixtures", "privacypass-ts.json")),
            new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
}

public sealed record PrivateKeyJwk(string N, string E, string D, string P, string Q, string Dp, string Dq, string Qi);
