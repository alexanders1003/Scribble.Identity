using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Scribble.Identity.Web.Definitions.Identity.Authorization;

namespace Scribble.Identity.Web.Definitions.Identity.Policy;

public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options) { }

    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith("Permissions", StringComparison.OrdinalIgnoreCase))
            return base.GetPolicyAsync(policyName);
        
        var builder = new AuthorizationPolicyBuilder();
        builder.AddRequirements(new PermissionRequirement(policyName));
        return Task.FromResult(builder.Build())!;
    }
}