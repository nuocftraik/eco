using System.Security.Claims;


namespace ECO.WebApi.Application.Common.Interfaces;
public interface ICurrentUserInitializer
{
    void SetCurrentUser(ClaimsPrincipal user);

    void SetCurrentUserId(string userId);
}
