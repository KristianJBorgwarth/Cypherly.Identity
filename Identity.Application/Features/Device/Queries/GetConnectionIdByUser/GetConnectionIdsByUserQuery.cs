using Identity.Application.Abstractions;

namespace Identity.Application.Features.Device.Queries.GetConnectionIdByUser;

public sealed record GetConnectionIdsByUserQuery : IQuery<GetConnectionIdsByUserDto>
{
    public required Guid UserId { get; init; }
}