using Cypherly.Domain.Common;
using Identity.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Behavior;

public class ExceptionBehavior<TRequest, TResponse>(
    ILogger<ExceptionBehavior<TRequest, TResponse>> logger) 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred while processing request of type {RequestType}", typeof(TRequest).Name);
            var error = Errors.General.UnspecifiedError("An unexpected error occurred. Please try again later.");
            return ResultFactory.Fail<TResponse>(error);
        }
    }
}
