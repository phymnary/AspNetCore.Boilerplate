using System.Net;

namespace AspNetCore.Boilerplate.Exceptions;

public class AspBadRequestException(string message) : Exception(message), IBusinessException
{
    public HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

    public string? ErrorCode { get; private set; }

    public AspBadRequestException WithErrorCode(string code)
    {
        ErrorCode = code;
        return this;
    }
}
