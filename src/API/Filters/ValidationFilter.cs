using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProductAPI.Application.Common;

namespace ProductAPI.API.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(ms => ms.Value?.Errors.Count > 0)
                .SelectMany(ms => ms.Value!.Errors.Select(e => e.ErrorMessage));

            var response = ApiResponse<object>.ErrorResult("Validation failed.", errors);
            context.Result = new BadRequestObjectResult(response);
            return;
        }

        await next();
    }
}