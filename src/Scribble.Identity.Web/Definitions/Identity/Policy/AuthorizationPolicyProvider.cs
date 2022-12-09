using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Scribble.Identity.Web.Definitions.Identity.Permissions;

namespace Scribble.Identity.Web.Definitions.Identity.Policy;

public class AuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private readonly AuthorizationOptions _options;

    public AuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options) => _options = options.Value;

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName)
            .ConfigureAwait(false);

        if (policy is not null) return policy;

        policy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName)).Build();
        
        _options.AddPolicy(policyName, policy);
        return policy;
    }
}