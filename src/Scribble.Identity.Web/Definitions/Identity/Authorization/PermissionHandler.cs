using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Scribble.Identity.Infrastructure;

namespace Scribble.Identity.Web.Definitions.Identity.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public PermissionHandler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.Identity is null)
        {
            context.Fail();
        }

        var user = await _userManager.GetUserAsync(context.User)
            .ConfigureAwait(false);

        if (user is null) return;
        
        var userRoles = await _userManager.GetRolesAsync(user!)
            .ConfigureAwait(false);

        var roles = _roleManager.Roles
            .Where(x => userRoles.Contains(x.Name!));

        foreach (var role in roles)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            var permissions = claims.Where(x => x.Type == PermissionClaimTypes.Permission
                                                && x.Value == requirement.PermissionName
                                                && x.Issuer == "LOCAL AUTHORITY")
                .Select(x => x.Value);

            if (permissions.Any())
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}