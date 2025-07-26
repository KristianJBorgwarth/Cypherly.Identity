using Identity.API.Filters;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Identity.Test.Integration.Setup.Authentication;

public class MockValidateUserIdIdFilter : IValidateUserIdFilter
{
    public new async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await next();
    }
}