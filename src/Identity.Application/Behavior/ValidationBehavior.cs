using Cypherly.Domain.Common;
using Identity.Domain.Common;
using Identity.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Identity.Application.Behavior
{
    public class ValidationBehavior<TRequest, TResponse>(
        ILogger<ValidationBehavior<TRequest, TResponse>> logger, 
        IEnumerable<IValidator<TRequest>> validators)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull, IMessage
        where TResponse : Result
    {
        public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
        {
            var validator = validators.FirstOrDefault();
            if (validator == null)
            {
                return await next(request, cancellationToken);
            }

            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (validationResult.IsValid)
            {
                return await next(request, cancellationToken);
            }

            var error = GenerateErrorMessage(validationResult);
            logger.LogWarning("Validation failed for {RequestType} with errors: {Errors}", typeof(TRequest).Name, error);

            return typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>)
                ? CreateGenericFailResponse(error)
                : CreateFailResponse(error);
        }

        private static Error GenerateErrorMessage(ValidationResult validationResult)
        {
            var errorMessage = string.Join("; ", validationResult.Errors.Select(x => x.ErrorMessage));
            return Errors.General.UnspecifiedError("Validation failed: " + errorMessage);
        }

        private static TResponse CreateFailResponse(Error error)
        {
            return (TResponse)Result.Fail(error);
        }

        private static TResponse CreateGenericFailResponse(Error error)
        {
            var resultType = typeof(TResponse).GetGenericArguments()[0];
            var method = typeof(Result).GetMethods()
                .FirstOrDefault(m => m is { Name: "Fail", IsGenericMethodDefinition: true })!;

            var genericFailMethod = method.MakeGenericMethod(resultType);
            return (TResponse)genericFailMethod.Invoke(null, [error])!;
        }
    }
}
