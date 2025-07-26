using Cypherly.Identity.Domain.Entities;

namespace Cypherly.Identity.Application.Features.Authentication.Commands.RefreshTokens;

public class RefreshTokensDto
{
    public required string Jwt { get; init; }
    public required string RefreshToken { get; init; }
    public DateTime ExpiresAt { get; init; }

    private RefreshTokensDto() { } // Hide the constructor to force the use of the Map method

    public static RefreshTokensDto Map(string jwt, RefreshToken refreshToken)
    {
        return new RefreshTokensDto()
        {
            Jwt = jwt,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.Expires,
        };
    }
}