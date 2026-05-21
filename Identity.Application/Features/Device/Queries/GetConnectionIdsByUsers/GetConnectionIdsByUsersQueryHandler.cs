using Cypherly.Domain.Common;
using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Identity.Application.Specifications;

namespace Identity.Application.Features.Device.Queries.GetConnectionIdsByUsers;

public class GetConnectionIdsByUsersQueryHandler(IUserRepository userRepository) : IQueryHandler<GetConnectionIdsByUsersQuery, GetConnectionIdsByUsersDto>
{
    public async Task<Result<GetConnectionIdsByUsersDto>> Handle(GetConnectionIdsByUsersQuery q, CancellationToken ct)
    {
        var users = await userRepository.GetListAsync(new UsersWithDevicesSpec(q.TenantIds), ct);

        var connectionIds = users.ToDictionary(
            user => user.Id,
            user => user.GetDevices().Select(device => device.ConnectionId).ToList());

        var dto = new GetConnectionIdsByUsersDto() { ConnectionIds = connectionIds };

        return dto;
    }
}
