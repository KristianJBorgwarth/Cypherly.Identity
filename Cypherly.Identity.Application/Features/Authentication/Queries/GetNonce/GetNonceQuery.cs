using Cypherly.Identity.Application.Abstractions;

namespace Cypherly.Identity.Application.Features.Authentication.Queries.GetNonce;

public sealed record GetNonceQuery : IQuery<GetNonceDto>
{
    public Guid UserId { get; init; }
    public Guid DeviceId { get; init; }
}