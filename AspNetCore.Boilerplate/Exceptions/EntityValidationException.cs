using System.Net;
using AspNetCore.Boilerplate.Api;
using FluentValidation.Results;

namespace AspNetCore.Boilerplate.Exceptions;

public class EntityValidationException(string message) : Exception(message), IBusinessException
{
    public HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public string? ErrorCode { get; private set; }

    public required AspValidationErrorDetail[] Failures { get; init; }

    public EntityValidationException WithErrorCode(string code)
    {
        ErrorCode = code;
        return this;
    }
}
