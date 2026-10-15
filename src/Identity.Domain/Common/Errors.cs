using Identity.Domain.ValueObjects;

namespace Identity.Domain.Common;

public class Errors
{
    public static class General
    {
        public static Error UnspecifiedError(string message) => new("unspecified.error", ErrorType.BadRequest, message);
        public static Error Validation(string description) => new("validation.error", ErrorType.Validation, description);
        public static Error Failure(string description = "An unexpected error occurred.") => new("internal.server.error", ErrorType.Failure, description);
        public static Error NotFound<T>(T id) => new("entity.not.found", ErrorType.NotFound, $"Could not find entity with ID {id}.");
        public static Error ValueIsRequired(string valueName) => new("value.is.required", ErrorType.Validation, $"Value '{valueName}' is required.");
        public static Error ValueTooSmall(string valueName, int minValue) => new("value.too.small", ErrorType.Validation, $"Value '{valueName}' should be at least {minValue}.");
        public static Error ValueTooLarge(string valueName, int maxValue) => new("value.too.large", ErrorType.Validation, $"Value '{valueName}' should not exceed {maxValue}.");
        public static Error UnexpectedValue(string value) => new("unexpected.value", ErrorType.Validation, $"Value '{value}' is not valid in this context");
        public static Error Unauthorized() => new("unauthorized", ErrorType.Unauthorized, "Could not authorize access to entity");
        public static Error ValueIsEmpty(string value) => new("value.empty", ErrorType.Validation, $"The value cannot be empty: {value} ");

        public static Error ValueOutOfRange(string valueName, int minValue, int maxValue) =>
            new("value.out.of.Range", ErrorType.Validation, $"Value '{valueName}' should be between {minValue} and {maxValue}.");

        public static Error TooManyRequests() =>
            new("too.many.requests", ErrorType.TooManyRequests, "Too many requests. Please try again later.");
    }
}
