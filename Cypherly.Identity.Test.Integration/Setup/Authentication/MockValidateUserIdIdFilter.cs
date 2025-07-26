using Cypherly.API.Filters;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Cypherly.Authentication.Test.Integration.Setup.Authentication;

public class MockValidateUserIdIdFilter : IValidateUserIdFilter
{
    public new async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await next();
    }
}