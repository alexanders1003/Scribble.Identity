using System.Security.Claims;

namespace Scribble.Identity.Web.Application.Managers.Base;

public interface IClaimsManager
{
    Task<ClaimsPrincipal> GetPrincipalByIdAsync(string id, CancellationToken token = default);
    Task<ClaimsPrincipal> GetPrincipalByEmailAsync(string email, CancellationToken token = default);
}