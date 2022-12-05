using System.Security.Claims;
using System.Security.Principal;

namespace Scribble.Identity.Web.Definitions.Identity;

public class IdentityHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private IIdentity? User =>
        _httpContextAccessor.HttpContext!.User.Identity != null
        && _httpContextAccessor.HttpContext != null
        && _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated
            ? _httpContextAccessor.HttpContext.User.Identity
            : null;

    public IEnumerable<Claim> Claims => 
        User != null 
            ? _httpContextAccessor.HttpContext!.User.Claims 
            : Enumerable.Empty<Claim>();
}