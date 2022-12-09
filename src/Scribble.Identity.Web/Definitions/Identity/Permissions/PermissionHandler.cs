using System.Security.Claims;
using Calabonga.Microservices.Core;
using Microsoft.AspNetCore.Authorization;

namespace Scribble.Identity.Web.Definitions.Identity.Permissions;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.Identity is null) return Task.CompletedTask;

        var identity = (ClaimsIdentity)context.User.Identity;
        var claim = ClaimsHelper.GetValue<string>(identity, requirement.PermissionName);

        if (claim is null) return Task.CompletedTask;

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}