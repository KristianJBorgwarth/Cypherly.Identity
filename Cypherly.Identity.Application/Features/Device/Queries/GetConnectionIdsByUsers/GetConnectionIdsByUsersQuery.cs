using Cypherly.Identity.Application.Abstractions;

namespace Cypherly.Identity.Application.Features.Device.Queries.GetConnectionIdsByUsers;

public sealed record GetConnectionIdsByUsersQuery : IQuery<GetConnectionIdsByUsersDto>
{
    public required IReadOnlyCollection<Guid> UserIds { get; init; }
}