using Identity.Application.Features.Authentication.Queries.GetJwks;

namespace Identity.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(Guid userId, Guid deviceId);
    Task<IReadOnlyList<JwksDto>> GenerateJwks(CancellationToken ct = default);
}
