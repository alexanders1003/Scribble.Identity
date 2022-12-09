using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;

namespace Scribble.Identity.Infrastructure;

public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    public ApplicationUserClaimsPrincipalFactory(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IOptions<IdentityOptions> options) 
        : base(userManager, roleManager, options) { }

    public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var principal = await base.CreateAsync(user);
        
        if (user.ApplicationUserProfile?.Permissions != null)
        {
            var permissions = user.ApplicationUserProfile.Permissions.ToList();
            if (permissions.Any())
            {
                permissions.ForEach(x => ((ClaimsIdentity)principal.Identity!)
                    .AddClaim(new Claim(x.PolicyName, nameof(x.PolicyName).ToLower())));
            }
        }
        
        if (!string.IsNullOrWhiteSpace(user.UserName))
            ((ClaimsIdentity)principal.Identity!)
                .AddClaim(new Claim(OpenIddictConstants.Claims.Username, user.UserName));

        if (!string.IsNullOrWhiteSpace(user.FirstName))
            ((ClaimsIdentity)principal.Identity!)
                .AddClaim(new Claim(OpenIddictConstants.Claims.GivenName, user.FirstName));
        
        if (!string.IsNullOrWhiteSpace(user.LastName))
            ((ClaimsIdentity)principal.Identity!)
                .AddClaim(new Claim(OpenIddictConstants.Claims.FamilyName, user.LastName));

        foreach (var principalClaim in principal.Claims)
            principalClaim.SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);

        return principal;
    }
}