using Cypherly.Domain.Common;
using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;

namespace Identity.Application.Features.Device.Queries.GetConnectionIdsByUsers;

public class GetConnectionIdsByUsersQueryHandler(IUserRepository userRepository) : IQueryHandler<GetConnectionIdsByUsersQuery, GetConnectionIdsByUsersDto>
{

    public async Task<Result<GetConnectionIdsByUsersDto>> Handle(GetConnectionIdsByUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetUsersAsync([.. request.TenantIds], cancellationToken);

        var connectionIds = users.ToDictionary(
            user => user.Id,
            user => user.GetDevices().Select(device => device.ConnectionId).ToList());

        var dto = new GetConnectionIdsByUsersDto() { ConnectionIds = connectionIds };

        return dto;
    }
}
