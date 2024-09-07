
using ECO.WebApi.Application.Common.Models;

namespace ECO.WebApi.Application.Identity.Users;
public class UserListFilter : PaginationFilter
{
    public bool? IsActive { get; set; }
}
