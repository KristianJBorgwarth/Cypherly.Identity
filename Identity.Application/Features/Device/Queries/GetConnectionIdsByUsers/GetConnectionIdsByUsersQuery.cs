using Identity.Application.Abstractions;

namespace Identity.Application.Features.Device.Queries.GetConnectionIdsByUsers;

public sealed record GetConnectionIdsByUsersQuery : IQuery<GetConnectionIdsByUsersDto>
{
    public required IReadOnlyCollection<Guid> UserIds { get; init; }
}