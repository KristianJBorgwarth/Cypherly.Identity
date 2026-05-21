using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Aggregates;
using Identity.Domain.Common;

namespace Identity.Application.Features.Device.Queries.GetConnectionIdByUser;

public class GetConnectionIdsByUserQueryHandler(IUserRepository userRepository) : IQueryHandler<GetConnectionIdsByUserQuery, GetConnectionIdsByUserDto>
{
    public async Task<Result<GetConnectionIdsByUserDto>> Handle(GetConnectionIdsByUserQuery q, CancellationToken ct)
    {
        var user = await userRepository.GetSinleAsync(new UserWithDevicesSpec(q.TenantId), ct);
        if (user is null) return Result.Fail<GetConnectionIdsByUserDto>(Error.NotFound<User>(q.TenantId.ToString()));

        var connectionIds = user.GetDevices().Select(x => x.ConnectionId).ToList();

        return new GetConnectionIdsByUserDto { ConnectionIds = connectionIds };
    }
}
