using Cypherly.Domain.Common;
using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Common;
using Identity.Domain.Events.Device;
using Identity.Domain.Services.User;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Device.Commands.Delete;

public sealed class DeleteDeviceCommandHandler(
        IUserRepository userRepository,
        IAuthenticationService authenticationService,
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

        var device = user.GetDevices().FirstOrDefault(d => d.Id == cmd.TargetDeviceId);
        if (device is null)
        {
            logger.LogError("Device with id {DeviceId} not found for user {UserId}", cmd.TargetDeviceId, cmd.UserId);
            return Result.Fail(Errors.General.NotFound(cmd.TargetDeviceId));
        }

        authenticationService.Logout(user, device.Id);

        user.AddDomainEvent(new DeviceDeletedEvent(cmd.UserId, device.Id));

        await unitOfWork.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
