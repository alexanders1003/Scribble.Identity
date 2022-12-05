using Microsoft.AspNetCore.Authorization;

namespace Scribble.Identity.Web.Definitions.Identity;

public class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permissionName) => PermissionName = permissionName;
    public string PermissionName { get; set; }
}