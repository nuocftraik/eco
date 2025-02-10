

using System.Net;

namespace ECO.WebApi.Domain.Common.Exceptions;
public class NotFoundException : DomainException
{
    public NotFoundException(string message)
        : base(message, null, HttpStatusCode.NotFound)
    {
    }
}
