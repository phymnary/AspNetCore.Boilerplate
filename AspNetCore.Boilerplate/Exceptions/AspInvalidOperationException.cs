using System.Net;

namespace AspNetCore.Boilerplate.Exceptions;

public class AspInvalidOperationException(string message) : Exception(message), IBusinessException
{
    public HttpStatusCode StatusCode => HttpStatusCode.UnprocessableEntity;

    public string? ErrorCode { get; private set; }

    public AspInvalidOperationException WithErrorCode(string code)
    {
        ErrorCode = code;
        return this;
    }
}
