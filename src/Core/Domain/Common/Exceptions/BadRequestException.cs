

using System.Net;

namespace ECO.WebApi.Domain.Common.Exceptions;
public class BadRequestException : DomainException
{
    public BadRequestException(string message)
          : base(message, null, HttpStatusCode.BadGateway)
    {
    }
}
