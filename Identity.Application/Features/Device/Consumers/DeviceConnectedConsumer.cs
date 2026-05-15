using Cypherly.Message.Contracts.Messages.Client;
using Identity.Application.Contracts.Repository;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Device.Consumers;

public class DeviceConnectedConsumer(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeviceConnectedConsumer> logger)
    : IConsumer<ClientConnectedMessage>
{
    public async Task Consume(ConsumeContext<ClientConnectedMessage> ctx)
    {
        try
        {
            var deviceId = ctx.Message.DeviceId;
            var user = await userRepository.GetSinleAsync(new UserByDeviceIdWithDevicesSpec(deviceId), ctx.CancellationToken);

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
            logger.LogError(ex, "Error processing device connected message for device {DeviceId}", ctx.Message.DeviceId);
            throw;
        }
    }
}
