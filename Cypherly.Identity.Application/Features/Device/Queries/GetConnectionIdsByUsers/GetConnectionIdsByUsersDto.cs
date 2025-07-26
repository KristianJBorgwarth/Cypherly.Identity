using System.Text.Json.Serialization;

namespace Cypherly.Identity.Application.Features.Device.Queries.GetConnectionIdsByUsers;

public sealed class GetConnectionIdsByUsersDto
{
    public Dictionary<Guid, List<Guid>> ConnectionIds { get; private init; }

    [JsonConstructor]
    private GetConnectionIdsByUsersDto(Dictionary<Guid, List<Guid>> connectionIds)
    {
        ConnectionIds = connectionIds;
    }

    public static GetConnectionIdsByUsersDto MapFrom(IReadOnlyCollection<Identity.Domain.Aggregates.User> users)
    {
        var connectionIds = users.ToDictionary(
            user => user.Id,
            user => user.GetDevices().Select(device => device.ConnectionId).ToList());

        return new GetConnectionIdsByUsersDto(connectionIds);
    }
}