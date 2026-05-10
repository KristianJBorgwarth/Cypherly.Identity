using Cypherly.Domain.Common;
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
    public async Task<Result<CreateDeviceDto>> Handle(CreateDeviceCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            logger.LogWarning("User {UserId} not found", request.UserId);
            return Result.Fail<CreateDeviceDto>(Errors.General.NotFound(request.UserId));
        }

        var loginNonce = await loginNonceCache.GetNonceAsync(request.LoginNonceId, cancellationToken);

        if (loginNonce is null || loginNonce.NonceValue != request.LoginNonce)
        {
            logger.LogWarning("Login nonce {LoginNonceId} not found or invalid for user with ID: {ID}", request.LoginNonceId, request.UserId);
            return Result.Fail<CreateDeviceDto>(Errors.General.Unauthorized());
        }

        var device = deviceService.RegisterDevice(user, request.Base64DevicePublicKey, request.DeviceAppVersion, request.DeviceType, request.DevicePlatform);

        var createClientResult = await CreateClient(device.Id, device.ConnectionId);

        if (createClientResult.Success is false)
            return Result.Fail<CreateDeviceDto>(createClientResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

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
        return Result.Fail(Errors.General.UnspecifiedError("Failed to create profile"));
    }
}
