using Identity.Domain.Entities;

namespace Identity.Application.Features.Authentication.Token;

public interface IJwtService
{
    string GenerateToken(Guid userId, Guid deviceId);
}