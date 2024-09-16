
using ECO.WebApi.Application.Identity.Users.Password;

namespace ECO.WebApi.Application.Identity.Users;
public interface IUserService : ITransientService
{   
    //Default
    Task<PaginationResponse<UserDetailDto>> SearchAsync(UserParameterFilter filter, CancellationToken cancellationToken);

    Task<bool> ExistsWithNameAsync(string name);
    Task<bool> ExistsWithEmailAsync(string email, string? exceptId = null);
    Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string? exceptId = null);
    Task<string> GetFullName(Guid userId);
    Task<List<UserDetailDto>> GetListAsync(CancellationToken cancellationToken);

    Task<int> GetCountAsync(CancellationToken cancellationToken);

    Task<UserDetailDto> GetAsync(string userId, CancellationToken cancellationToken);

    //Role
    Task<List<UserRoleDto>> GetRolesAsync(string userId, CancellationToken cancellationToken);
    Task<string> AssignRolesAsync(string userId, UserRolesRequest request, CancellationToken cancellationToken);

    //Permission
    Task<List<string>> GetPermissionsAsync(string userId, CancellationToken cancellationToken);
    Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default);

    //Create Update
    Task ToggleStatusAsync(ToggleUserStatusRequest request, CancellationToken cancellationToken);

    Task<string> CreateAsync(CreateUserRequest request, string origin);
    Task UpdateAsync(UpdateUserRequest request, string userId);

    //Confirm
    Task<string> ConfirmEmailAsync(string userId, string code, CancellationToken cancellationToken);
    Task<string> ConfirmPhoneNumberAsync(string userId, string code);


    //Password
    Task<string> ForgotPasswordAsync(ForgotPasswordRequest request, string origin);
    Task<string> ResetPasswordAsync(ResetPasswordRequest request);
    Task ChangePasswordAsync(ChangePasswordRequest request, string userId);
}
