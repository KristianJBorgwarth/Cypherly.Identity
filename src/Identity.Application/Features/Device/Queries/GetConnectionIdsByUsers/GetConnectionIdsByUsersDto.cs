using System.Text.Json.Serialization;

namespace Identity.Application.Features.Device.Queries.GetConnectionIdsByUsers;

public sealed class GetConnectionIdsByUsersDto
{
    public Dictionary<Guid, List<Guid>> ConnectionIds { get; init; } = new();
}