using Cypherly.Identity.Application.Contracts.Repository;
using Cypherly.Message.Contracts.Messages.Client;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Cypherly.Identity.Application.Features.Device.Consumers;

public class DeviceConnectedConsumer(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeviceConnectedConsumer> logger)
    : IConsumer<ClientConnectedMessage>
{
    public async Task Consume(ConsumeContext<ClientConnectedMessage> context)
    {
        try
        {
            var deviceId = context.Message.DeviceId;
            var user = await userRepository.GetByDeviceIdAsync(deviceId);

            if (user is null)
            {
                logger.LogWarning("User not found for device {DeviceId}", deviceId);
                throw new InvalidOperationException("User not found for device");
            }

            var device = user.GetDevice(deviceId);

            device.SetLastSeen();

            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing device connected message for device {DeviceId}", context.Message.DeviceId);
            throw;
        }
    }
}