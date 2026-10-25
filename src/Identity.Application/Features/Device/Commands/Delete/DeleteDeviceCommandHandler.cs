using Cypherly.Domain.Common;
using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Common;
using Identity.Domain.Events.Device;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Device.Commands.Delete;

public sealed class DeleteDeviceCommandHandler(
        IUserRepository userRepository,
        ILogger<DeleteDeviceCommandHandler> logger,
        IUnitOfWork unitOfWork)
        : ICommandHandler<DeleteDeviceCommand>
{
    public async ValueTask<Result> Handle(DeleteDeviceCommand cmd, CancellationToken ct)
    {
        var user = await userRepository.GetSinleAsync(new UserWithDevicesSpec(cmd.UserId), ct);
        if (user is null)
        {
            logger.LogError("User with id {UserId} not found", cmd.UserId);
            return Result.Fail(Errors.General.NotFound(cmd.UserId));
        }

        var device = user.GetDevice(cmd.TargetDeviceId, true);
        if (device is null)
        {
            logger.LogError("Device with id {DeviceId} not found for user {UserId}", cmd.TargetDeviceId, cmd.UserId);
            return Result.Fail(Errors.General.NotFound(cmd.TargetDeviceId));
        }

        device.SetDelete();

        user.AddDomainEvent(new DeviceDeletedEvent(cmd.UserId, cmd.TargetDeviceId));

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
