using System.Security.Claims;
using Scribble.Identity.Web.Models.Account;

namespace Scribble.Identity.Web.Application.Managers.Base;

public interface IAccountManager
{
    Task<UserViewModel> RegisterAsync(RegisterViewModel model, CancellationToken token);
    Task<UserViewModel> GetUserByIdAsync(string id, CancellationToken token);
    Task<UserViewModel> GetUserByEmailAsync(string email, CancellationToken token);
    IEnumerable<string> GetUserRoles(ClaimsPrincipal principal);
}