

using System.Net;

namespace ECO.WebApi.Domain.Common.Exceptions;
public class DomainException : Exception
{
    public List<string>? ErrorMessages { get; }

    public HttpStatusCode StatusCode { get; }

    public DomainException(string message, List<string>? errors = default, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
      : base(message)
    {
        ErrorMessages = errors;
        StatusCode = statusCode;
    }


}

