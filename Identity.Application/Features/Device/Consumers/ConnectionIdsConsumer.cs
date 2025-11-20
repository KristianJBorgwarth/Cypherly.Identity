using Cypherly.Message.Contracts.Messages.Device;
using Cypherly.Message.Contracts.Responses.Device;
using Identity.Application.Contracts.Repository;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Device.Consumers;

public sealed class ConnectionIdsConsumer(
    IUserRepository userRepository,
    ILogger<ConnectionIdsConsumer> logger)
    : IConsumer<ConnectionIdsMessage>
{
    public async Task Consume(ConsumeContext<ConnectionIdsMessage> context)
    {
        try
        {
            var ids = context.Message.TenantIds.ToArray();
            var users = await userRepository.GetUsersAsync(ids);

            var connectionIds = users.ToDictionary(
                user => user.Id,
                user => user.GetDevices().Select(device => device.ConnectionId).ToList());

            await context.RespondAsync(new ConnectionIdsResponse
            {
                CorrelationId = context.Message.CorrelationId,
                CausationId = context.Message.CausationId,
                IsSuccess = true,
                ConnectionIds = connectionIds
            });

            logger.LogInformation("Successfully retrieved connection ids for users {TenantIds}", string.Join(", ", ids));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occured while attempting to get connection ids by users");
            throw;
        }
    }
}