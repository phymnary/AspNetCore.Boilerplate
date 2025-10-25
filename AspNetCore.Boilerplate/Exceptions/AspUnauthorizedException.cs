using System.Net;

namespace AspNetCore.Boilerplate.Exceptions;

public class AspUnauthorizedException(string message) : Exception(message), IBusinessException
{
    public HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;

    public string? ErrorCode { get; private set; }

    public AspUnauthorizedException WithErrorCode(string code)
    {
        ErrorCode = code;
        return this;
    }
}
