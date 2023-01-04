using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Scribble.Identity.Infrastructure.Exceptions;

namespace Scribble.Identity.Infrastructure;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class ClaimsManager<TUser> where TUser : class
{
    private readonly UserManager<TUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<TUser> _claimsFactory;

    public ClaimsManager(UserManager<TUser> userManager, 
        IUserClaimsPrincipalFactory<TUser> claimsFactory)
    {
        _userManager = userManager;
        _claimsFactory = claimsFactory;
    }

    public virtual async Task<ClaimsPrincipal> GetPrincipalByIdAsync(string id, CancellationToken token = default)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException($"Parameter {nameof(id)} was invalid.", nameof(id));

        var user = await _userManager.FindByIdAsync(id)
            .ConfigureAwait(false);

        if (user == null)
            throw new MicroserviceIdentityNotFoundException(user!.GetType());

        return await _claimsFactory.CreateAsync(user)
            .ConfigureAwait(false);
    }

    public virtual async Task<ClaimsPrincipal> GetPrincipalByEmailAsync(string email, CancellationToken token = default)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException($"Parameter {nameof(email)} was invalid.", nameof(email));

        var user = await _userManager.FindByEmailAsync(email)
            .ConfigureAwait(false);

        if (user == null)
            throw new MicroserviceIdentityNotFoundException(user!.GetType());

        return await _claimsFactory.CreateAsync(user)
            .ConfigureAwait(false);    
    }
}