namespace Identity.Application.Features.Device.Queries.GetConnectionIdByUser;

public sealed record GetConnectionIdsByUserDto
{
    public required IReadOnlyCollection<Guid> ConnectionIds { get; init; } = [];
}