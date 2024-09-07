
using ECO.WebApi.Application.Common.Models;

namespace ECO.WebApi.Application.Identity.Users;
public class UserParameterFilter : PaginationFilter
{
    public bool? IsActive { get; set; }
}
