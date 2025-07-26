using Cypherly.Identity.Domain.Entities;

namespace Cypherly.Identity.Application.Features.Authentication.Token;

public interface IJwtService
{
    string GenerateToken(Guid userId, Guid deviceId);
}