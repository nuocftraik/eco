
using System.Net;


namespace ECO.WebApi.Application.Common.Exceptions;
public class ConflictException : CustomException
{
    public ConflictException(string message)
        : base(message, null, HttpStatusCode.Conflict)
    {
    }
}
