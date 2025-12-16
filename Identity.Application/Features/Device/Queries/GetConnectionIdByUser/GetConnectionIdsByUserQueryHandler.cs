using Cypherly.Domain.Common;
using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Common;

namespace Identity.Application.Features.Device.Queries.GetConnectionIdByUser;

public class GetConnectionIdsByUserQueryHandler(IUserRepository userRepository) : IQueryHandler<GetConnectionIdsByUserQuery, GetConnectionIdsByUserDto>
{
    public async Task<Result<GetConnectionIdsByUserDto>> Handle(GetConnectionIdsByUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.TenantId);
        if (user is null) return Result.Fail<GetConnectionIdsByUserDto>(Errors.General.NotFound(request.TenantId));

        var connectionIds = user.GetDevices().Select(x => x.ConnectionId).ToList();

        return new GetConnectionIdsByUserDto { ConnectionIds = connectionIds };
    }
}
