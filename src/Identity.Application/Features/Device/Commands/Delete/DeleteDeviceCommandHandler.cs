using Cypherly.Domain.Common;
using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Features.Device.Commands.Delete;

public sealed class DeleteDeviceCommandHandler(
        IUserRepository userRepository,
        ILogger<DeleteDeviceCommandHandler> logger) 
        : ICommandHandler<DeleteDeviceCommand>
{
    public ValueTask<Result> Handle(DeleteDeviceCommand cmd, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
