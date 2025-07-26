using Cypherly.Identity.Application.Caching.LoginNonce;

namespace Cypherly.Identity.Application.Features.Authentication.Commands.VerifyLogin;

public sealed class VerifyLoginDto
{
    public Guid NonceId { get; private init; }
    public string Nonce { get; private init; } = null!;

    private VerifyLoginDto() { } // Hide the constructor to force the use of the factory methods

    public static VerifyLoginDto Map(LoginNonce nonce)
    {
        return new VerifyLoginDto()
        {
            NonceId = nonce.Id,
            Nonce = nonce.NonceValue,
        };
    }
}