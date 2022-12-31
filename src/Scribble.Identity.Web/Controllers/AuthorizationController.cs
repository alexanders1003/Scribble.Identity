using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Scribble.Identity.Infrastructure;

namespace Scribble.Identity.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class AuthorizationController : Controller
{
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ClaimsManager<ApplicationUser> _claimsManager;

    public AuthorizationController(IOpenIddictScopeManager scopeManager, 
        IOpenIddictApplicationManager applicationManager, 
        IOpenIddictAuthorizationManager authorizationManager, 
        UserManager<ApplicationUser> userManager, 
        SignInManager<ApplicationUser> signInManager, 
        ClaimsManager<ApplicationUser> claimsManager)
    {
        _scopeManager = scopeManager;
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _userManager = userManager;
        _signInManager = signInManager;
        _claimsManager = claimsManager;
    }
    
    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.Request;
        var iRequest = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!result.Succeeded)
        {
            return Challenge(
                authenticationSchemes: new[] { CookieAuthenticationDefaults.AuthenticationScheme },
                properties: new AuthenticationProperties
                {
                    RedirectUri = request.PathBase + request.Path + QueryString.Create(request.HasFormContentType
                        ? request.Form.ToList()
                        : request.Query.ToList())
                }
            );
        }

        var user = await _userManager.GetUserAsync(result.Principal)
                   ?? throw new InvalidOperationException("The user details cannot be retrieved.");

        var application = await _applicationManager.FindByClientIdAsync(iRequest.ClientId!)
                          ?? throw new InvalidOperationException(
                              "Details concerning the calling client application cannot be found.");
        var applicationId = await _applicationManager.GetIdAsync(application);
        var userId = await _userManager.GetUserIdAsync(user);

        var authorizations = await _authorizationManager.FindAsync(
            subject: userId,
            client: applicationId!,
            status: OpenIddictConstants.Statuses.Valid,
            type: OpenIddictConstants.AuthorizationTypes.Permanent,
            scopes: iRequest.GetScopes()).ToListAsync();

        switch (await _applicationManager.GetConsentTypeAsync(application))
        {
            case OpenIddictConstants.ConsentTypes.External when !authorizations.Any():
                return Forbid(
                    authenticationSchemes: new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme },
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The logged in user is not allowed to access this client application."
                    }!));

            case OpenIddictConstants.ConsentTypes.Implicit:
            case OpenIddictConstants.ConsentTypes.External when authorizations.Any():
            case OpenIddictConstants.ConsentTypes.Explicit
                when authorizations.Any() && !iRequest.HasPrompt(OpenIddictConstants.Prompts.Consent):

                var principal = await _signInManager.CreateUserPrincipalAsync(user);

                var sc = iRequest.GetScopes();
                principal.SetScopes(sc);
                principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

                var authorization = authorizations.LastOrDefault() ?? await _authorizationManager.CreateAsync(
                    principal: principal,
                    subject: await _userManager.GetUserIdAsync(user),
                    client: applicationId!,
                    type: OpenIddictConstants.AuthorizationTypes.Permanent,
                    scopes: principal.GetScopes());

                principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

                foreach (var claim in principal.Claims)
                {
                    claim.SetDestinations(claim.Type switch
                    {
                        OpenIddictConstants.Claims.Name when principal.HasScope(OpenIddictConstants.Scopes.Profile) =>
                            new[]
                            {
                                OpenIddictConstants.Destinations.AccessToken,
                                OpenIddictConstants.Destinations.IdentityToken
                            },

                        "secret_value" => Array.Empty<string>(),

                        _ => new[]
                        {
                            OpenIddictConstants.Destinations.AccessToken
                        }
                    });
                }

                return SignIn(principal, null!, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            case OpenIddictConstants.ConsentTypes.Explicit when iRequest.HasPrompt(OpenIddictConstants.Prompts.None):
            case OpenIddictConstants.ConsentTypes.Systematic when iRequest.HasPrompt(OpenIddictConstants.Prompts.None):
                return Forbid(
                    authenticationSchemes: new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme },
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "Interactive user consent is required."
                    }!));

            default:
                return Challenge(
                    authenticationSchemes: new[] { OpenIddictServerAspNetCoreDefaults.AuthenticationScheme },
                    properties: new AuthenticationProperties { RedirectUri = "/" });
        }
    }
    
    [HttpPost("~/connect/token"), ExcludeFromDescription]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest() 
                      ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsClientCredentialsGrantType())
        {
            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            identity.AddClaim(OpenIddictConstants.Claims.Subject, request.ClientId!);
            identity.AddClaim(OpenIddictConstants.Claims.ClientId, request.ClientId!);

            identity.AddClaim(OpenIddictConstants.Claims.Scope, request.Scope!, OpenIddictConstants.Destinations.AccessToken);

            var claimsPrincipal = new ClaimsPrincipal(identity);

            claimsPrincipal.SetScopes(request.GetScopes());
            
            return SignIn(claimsPrincipal, new AuthenticationProperties(), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsPasswordGrantType())
        {
            var user = await _userManager.FindByNameAsync(request.Username!);
            if (user == null)
                return Problem("Invalid operation");

            if (!await _signInManager.CanSignInAsync(user))
                return Problem("Invalid operation");

            if (_userManager.SupportsUserLockout && await _userManager.IsLockedOutAsync(user))
                return Problem("Invalid operation");

            if (!await _userManager.CheckPasswordAsync(user, request.Password!))
            {
                if (_userManager.SupportsUserLockout)
                {
                    await _userManager.AccessFailedAsync(user);
                }

                return Problem("Invalid operation");
            }

            if (_userManager.SupportsUserLockout)
                await _userManager.ResetAccessFailedCountAsync(user);

            var principal = await _claimsManager.GetPrincipalByEmailAsync(user.Email!, HttpContext.RequestAborted);
            return SignIn(principal, new AuthenticationProperties(), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsAuthorizationCodeGrantType())
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var properties = authenticateResult.Properties;
            var claimsPrincipal = authenticateResult.Principal;
            return SignIn(claimsPrincipal!, properties ?? new AuthenticationProperties(), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return Problem("The specified grant type is not supported.");
    }
}