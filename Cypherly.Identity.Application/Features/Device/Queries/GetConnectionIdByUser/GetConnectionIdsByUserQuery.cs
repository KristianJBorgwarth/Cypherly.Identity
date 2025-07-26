using Cypherly.Identity.Application.Abstractions;

namespace Cypherly.Identity.Application.Features.Device.Queries.GetConnectionIdByUser;

public sealed record GetConnectionIdsByUserQuery : IQuery<GetConnectionIdsByUserDto>
{
    public required Guid UserId { get; init; }
}