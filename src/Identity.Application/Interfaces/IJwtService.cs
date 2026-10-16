using Identity.Application.Dtos;

namespace Identity.Application.Interfaces;

public interface IJwtService
{
    Task<AccessToken> GenerateTokenAsync(Guid userId, Guid deviceId, CancellationToken ct = default);
    Task<IReadOnlyList<JwksDto>> GenerateJwks(CancellationToken ct = default);
}
