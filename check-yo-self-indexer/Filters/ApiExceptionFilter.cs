using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace check_yo_self_indexer.Server.Filters;

public class ApiExceptionFilter : ExceptionFilterAttribute
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger;
    }


    public override void OnException(ExceptionContext context)
    {
        ApiError apiError;
        if (context.Exception is ApiException)
        {
            // handle explicit 'known' API errors
            var ex = context.Exception as ApiException;
            context.Exception = null;
            apiError = new ApiError(ex.Message)
            {
                Errors = ex.Errors
            };

            context.HttpContext.Response.StatusCode = ex.StatusCode;

            _logger.LogWarning(ex, "Application thrown error: {message}", ex.Message);
        }
        else if (context.Exception is UnauthorizedAccessException)
        {
            apiError = new ApiError("Unauthorized Access");
            context.HttpContext.Response.StatusCode = 401;
            _logger.LogWarning("Unauthorized Access in Controller Filter.");
        }
        else
        {
            // Unhandled errors
            var msg = "An unhandled error occurred.";
            string stack = null;

            apiError = new ApiError(msg)
            {
                Detail = stack
            };

            context.HttpContext.Response.StatusCode = 500;

            // handle logging here
            _logger.LogError(context.Exception, "{message}", msg);
        }

        // always return a JSON result
        context.Result = new JsonResult(apiError);

        base.OnException(context);
    }
}
