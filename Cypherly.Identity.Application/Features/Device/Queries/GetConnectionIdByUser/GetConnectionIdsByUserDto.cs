namespace Cypherly.Identity.Application.Features.Device.Queries.GetConnectionIdByUser;

public sealed class GetConnectionIdsByUserDto
{
    public required IReadOnlyCollection<Guid> ConnectionIds { get; init; } = [];

    private GetConnectionIdsByUserDto() { }

    public static GetConnectionIdsByUserDto MapFrom(List<Identity.Domain.Entities.Device> devices)
    {
        return new GetConnectionIdsByUserDto()
        {
            ConnectionIds = devices.Count > 0 ? devices.Select(x => x.ConnectionId).ToList() : [],
        };
    }
}