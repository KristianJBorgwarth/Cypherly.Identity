using Identity.Domain.Common;
using Identity.Domain.Services.User;
using Cypherly.Message.Contracts.Messages.Client;
using Cypherly.Message.Contracts.Responses.Client;
using Identity.Application.Abstractions;
using Identity.Application.Contracts.Cache;
using Identity.Application.Contracts.Repository;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Device.Commands.Create;

public class CreateDeviceCommandHandler(
    IUserRepository userRepository,
    IRequestClient<CreateClientMessage> requestClient,
    ILoginNonceCache loginNonceCache,
    IDeviceService deviceService,
    IUnitOfWork unitOfWork,
    ILogger<CreateDeviceCommandHandler> logger)
    : ICommandHandler<CreateDeviceCommand, CreateDeviceDto>
{
    public async Task<Result<CreateDeviceDto>> Handle(CreateDeviceCommand cmd, CancellationToken ct)
    {
        var user = await userRepository.GetSinleAsync(new UserWithDevicesSpec(cmd.UserId), ct);
        if (user is null)
        {
            logger.LogWarning("User {UserId} not found", cmd.UserId);
            return Result.Fail<CreateDeviceDto>(Error.NotFound<Identity.Domain.Aggregates.User>(cmd.UserId.ToString()));
        }

        var loginNonce = await loginNonceCache.GetNonceAsync(cmd.LoginNonceId, ct);

        if (loginNonce is null || loginNonce.NonceValue != cmd.LoginNonce)
        {
            logger.LogWarning("Login nonce {LoginNonceId} not found or invalid for user with ID: {ID}", cmd.LoginNonceId, cmd.UserId);
            return Result.Fail<CreateDeviceDto>(Error.Unauthorized());
        }

        var device = deviceService.RegisterDevice(user, cmd.Base64DevicePublicKey, cmd.DeviceAppVersion, cmd.DeviceType, cmd.DevicePlatform);

        var createClientResult = await CreateClient(device.Id, device.ConnectionId);

        if (createClientResult.Success is false)
            return Result.Fail<CreateDeviceDto>(createClientResult.Error);

        await unitOfWork.SaveChangesAsync(ct);

        var dto = CreateDeviceDto.Map(device);

        return Result.Ok(dto);
    }

    private async Task<Result> CreateClient(Guid deviceId, Guid connectionId)
    {
        var response = await requestClient.GetResponse<CreateClientResponse>(new CreateClientMessage
        {
            CorrelationId = Guid.NewGuid(),
            DeviceId = deviceId,
            ConnectionId = connectionId
        });

        if (response.Message.IsSuccess)
            return Result.Ok();

        logger.LogError("Got a fail response from the Chat server attempting to create a Clients for Device with ID {DeviceId}", deviceId);
        return Result.Fail(Error.Failure("Failed to create profile"));
    }
}
