using Cypherly.Message.Contracts.Messages.Device;
using Cypherly.Message.Contracts.Responses.Device;
using Identity.Application.Contracts.Repository;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Device.Consumers;

public sealed class ConnectionIdConsumer(
    IUserRepository userRepository,
    ILogger<ConnectionIdConsumer> logger)
    : IConsumer<ConnectionIdMessage>
{
    public async Task Consume(ConsumeContext<ConnectionIdMessage> ctx)
    {
        try
        {
            var msg = ctx.Message;
            var user = await userRepository.GetSinleAsync(new UserWithDevicesSpec(msg.TenantId), ctx.CancellationToken);

            if (user is null)
            {
                logger.LogWarning("User not found for tenant {TenantId}", msg.TenantId);
                throw new InvalidOperationException("User not found for tenant");
            }

            var connectionIds = user.GetDevices().Select(x => x.ConnectionId).ToList();

            await ctx.RespondAsync(new ConnectionIdResponse
            {
                CorrelationId = msg.CorrelationId,
                CausationId = msg.CausationId,
                IsSuccess = true,
                ConnectionIds = connectionIds
            });

            logger.LogInformation("Successfully retrieved connection ids for user {TenantId}", msg.TenantId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occured while attempting to get connection ids by user");
            throw;
        }
    }
}
