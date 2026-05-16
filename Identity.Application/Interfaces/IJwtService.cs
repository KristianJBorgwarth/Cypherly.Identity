using Identity.Application.Dtos;

namespace Identity.Application.Interfaces;

public interface IJwtService
{
    Task<string> GenerateTokenAsync(Guid userId, Guid deviceId, CancellationToken ct = default);
    Task<IReadOnlyList<JwksDto>> GenerateJwks(CancellationToken ct = default);
}
