using System.Net;

namespace AspNetCore.Boilerplate.Exceptions;

public class EntityNotFoundException(string message) : Exception(message), IBusinessException
{
    public HttpStatusCode StatusCode => HttpStatusCode.NotFound;

    public string? ErrorCode { get; private set; }

    public EntityNotFoundException WithErrorCode(string code)
    {
        ErrorCode = code;
        return this;
    }
}
