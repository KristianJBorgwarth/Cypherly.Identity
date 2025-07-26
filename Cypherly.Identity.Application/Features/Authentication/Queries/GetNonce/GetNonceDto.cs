using Cypherly.Identity.Application.Caching;

namespace Cypherly.Identity.Application.Features.Authentication.Queries.GetNonce;

public sealed record GetNonceDto
{
    public Guid NonceId { get; private init; }
    public Guid DeviceId { get; private init; }
    public string NonceValue { get; private init; } = null!;
    private GetNonceDto() { } // Hide the constructor to force the use of the Map method

    public static GetNonceDto Map(Nonce nonce)
    {
        return new GetNonceDto()
        {
            NonceId = nonce.Id,
            DeviceId = nonce.DeviceId,
            NonceValue = nonce.NonceValue
        };
    }
}