using Cypherly.Message.Contracts.Abstractions;
using Cypherly.Message.Contracts.Messages.Device;
using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Events.Device;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Device.Events;

public sealed class DeviceDeletedEventHandler(
    IUserRepository userRepository,
    IProducer<DeviceDeletedMessage> deviceDeletedProducer,
    ILogger<DeviceDeletedEventHandler> logger)
    : IDomainEventHandler<DeviceDeletedEvent>
{
    public async ValueTask Handle(DeviceDeletedEvent ntf, CancellationToken ct)
    {
        var user = await userRepository.GetSinleAsync(new UserWithDevicesSpec(ntf.UserId), ct);
        if (user is null)
        {
            logger.LogError("User with id {UserId} not found", ntf.UserId);
            throw new InvalidOperationException("User not found");
        }

        var device = user.GetDevice(ntf.DeviceId, true);

        var deviceDeletedMessage = new DeviceDeletedMessage
        {
            CorrelationId = Guid.NewGuid(),
            DeviceId = device.Id,
        };

        await deviceDeletedProducer.PublishMessageAsync(deviceDeletedMessage, ct);
    }
}
