using Cypherly.Identity.Domain.Entities;

namespace Cypherly.Identity.Application.Features.Authentication.Commands.VerifyNonce;

public class VerifyNonceDto
{
    public required string Jwt { get; init; }
    public required string RefreshToken { get; init; }
    public DateTime ExpiresAt { get; init; }

    private VerifyNonceDto() { } // Hide the constructor to force the use of the Map method

    public static VerifyNonceDto Map(string jwt, RefreshToken refreshToken)
    {
        return new VerifyNonceDto()
        {
            Jwt = jwt,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.Expires,
        };
    }
}