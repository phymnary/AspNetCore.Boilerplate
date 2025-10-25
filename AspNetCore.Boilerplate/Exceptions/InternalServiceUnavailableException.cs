using System.Net;

namespace AspNetCore.Boilerplate.Exceptions;

public class InternalServiceUnavailableException(string message)
    : Exception(message),
        IBusinessException
{
    public HttpStatusCode StatusCode => HttpStatusCode.ServiceUnavailable;

    public string? ErrorCode { get; private set; }

    public InternalServiceUnavailableException WithErrorCode(string code)
    {
        ErrorCode = code;
        return this;
    }
}
