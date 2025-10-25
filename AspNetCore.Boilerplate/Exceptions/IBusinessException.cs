using System.Net;

namespace AspNetCore.Boilerplate.Exceptions;

public interface IBusinessException
{
    HttpStatusCode StatusCode { get; }

    string? ErrorCode => null;
}
