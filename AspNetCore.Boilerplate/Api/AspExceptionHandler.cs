using System.Net;
using System.Net.Mime;
using System.Text.Json;
using AspNetCore.Boilerplate.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ValidationFailure = FluentValidation.Results.ValidationFailure;

namespace AspNetCore.Boilerplate.Api;

public class AspExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        HttpStatusCode status;
        string? errorCode = null;
        string? details = null;
        IEnumerable<AspValidationErrorDetail>? validationErrors = null;

        switch (exception)
        {
            case IBusinessException businessException:
                status = businessException.StatusCode;
                errorCode = businessException.ErrorCode;
                if (businessException is EntityValidationException validationException)
                    validationErrors = validationException.Failures;
                break;
            case DbUpdateException:
                status = HttpStatusCode.FailedDependency;
                break;
            default:
                status = HttpStatusCode.InternalServerError;
                break;
        }

        httpContext.Response.StatusCode = (int)status;
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(
                new AspErrorDto
                {
                    Error = new AspErrorMessage
                    {
                        Code = errorCode,
                        Message = exception.Message,
                        Detail = details,
                        InvalidParameters = validationErrors,
                    },
                },
                AspBoilerplateJsonSerializerContext.Default.AspErrorDto
            ),
            cancellationToken
        );

        return true;
    }
}
