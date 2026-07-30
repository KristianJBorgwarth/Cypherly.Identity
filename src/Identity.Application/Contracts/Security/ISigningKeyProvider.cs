using Identity.Application.Dtos;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Application.Contracts.Security;

/// <param name="Kid">Goes into the JWT header so validators know which JWKS entry to verify against.</param>
/// <param name="Alg">The algorithm this key is pinned to, e.g. RS256.</param>
public sealed record ActiveSigningKey(string Kid, string Alg, RsaSecurityKey Key);

public interface ISigningKeyProvider
{
    /// <summary>
    /// The key to sign new tokens with.
    /// </summary>
    Task<ActiveSigningKey> GetCurrentAsync(CancellationToken ct = default);

    /// <summary>
    /// Everything that belongs in the JWKS: the current key plus retired keys whose
    /// tokens may still be in flight. Public members only.
    /// </summary>
    Task<IReadOnlyList<JwksDto>> GetPublishedAsync(CancellationToken ct = default);
}
