using Microsoft.IdentityModel.Tokens;

public interface IJwkCache
{
    Task<JsonWebKeySet> GetJwks(CancellationToken ct = default);
}
