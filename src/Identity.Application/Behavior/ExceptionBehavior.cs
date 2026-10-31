using Cendrite.Domain.Common;
using Identity.Domain.Common;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Behavior;

public class ExceptionBehavior<TRequest, TResponse>(
    ILogger<ExceptionBehavior<TRequest, TResponse>> logger) 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IMessage
    where TResponse : Result
{
    public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(request, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred while processing request of type {RequestType}", typeof(TRequest).Name);
            var error = Errors.General.Failure("An unexpected error occurred. Please try again later.");
            return ResultFactory.Fail<TResponse>(error);
        }
    }
}
