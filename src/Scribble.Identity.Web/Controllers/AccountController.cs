using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Server.AspNetCore;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Web.Application.Managers.Base;
using Scribble.Identity.Web.Definitions.OpenIddict;
using Scribble.Identity.Web.Models.Account;

namespace Scribble.Identity.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize(AuthenticationSchemes = AuthenticationData.AuthenticationSchemes)]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IClaimsManager _claimsManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IClaimsManager claimsManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _claimsManager = claimsManager;
    }

    [AllowAnonymous]
    [HttpGet("~/connect/login")]
    public IActionResult Login(string returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [AllowAnonymous, ValidateAntiForgeryToken]
    [HttpPost("~/connect/login")]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email!)
            .ConfigureAwait(false);

        if (user == null)
        {
            ModelState.AddModelError("UserNotFound", "User not found");
            return View(model);
        }

        var signInResult = await _signInManager
            .PasswordSignInAsync(user, model.Password!, model.RememberMe, false)
            .ConfigureAwait(false);

        if (signInResult.Succeeded)
        {
            var principal = await _claimsManager.GetPrincipalByIdAsync(user.Id.ToString())
                .ConfigureAwait(false);
           
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal)
                .ConfigureAwait(false);

            return Redirect(returnUrl);
        }

        if (signInResult.IsNotAllowed)
        {
            ModelState.AddModelError("IsNotAllowed", "Sign-in is not allowed for this account.");
            return View(model);
        }

        return View(model);
    }

    [AllowAnonymous]
    [HttpGet("~/connect/register")]
    public IActionResult Register(string returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [AllowAnonymous, ValidateAntiForgeryToken]
    [HttpPost("~/connect/register")]
    public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password)
            .ConfigureAwait(false);

        if (result.Succeeded)
        {
            var signInResult = await _signInManager
                .PasswordSignInAsync(user, model.Password, true, false)
                .ConfigureAwait(false);

            if (signInResult.Succeeded)
            {
                var principal = await _claimsManager.GetPrincipalByEmailAsync(user.Email)
                    .ConfigureAwait(false);
                
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal)
                    .ConfigureAwait(false);

                return Redirect(returnUrl);
            }
        }

        AddErrors(result);
        
        return View(model);
    }

    [ValidateAntiForgeryToken]
    [HttpPost("~/connect/logout")]
    public async Task<IActionResult> Logout(string returnUrl)
    {
        await _signInManager.SignOutAsync()
            .ConfigureAwait(false);

        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties
            {
                RedirectUri = returnUrl
            });
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }
    }
}