using Identity.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Common;

public static class ResultExtensions
{
    public static IActionResult ToProblemDetails(this Result result)
    {
        if (result.Success)
            throw new InvalidOperationException("Cannot convert a successful result to problem details");

        return new ObjectResult(new ProblemDetails
        {
            Status = result.Error?.Type.ToHttpStatusCode(),
            Type = result.Error?.Type.ToProblemDetailsTypeUri(),
            Title = result.Error?.Type.ToProblemDetailsTitle(),
            Extensions = { ["errors"] = new[] { result.Error } }
        })
        {
            StatusCode = result.Error?.Type.ToHttpStatusCode()
        };
    }
}
