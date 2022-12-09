using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Web.Application.Managers;
using Scribble.Identity.Web.Definitions.OpenIddict;
using Scribble.Identity.Web.Infrastructure.Extensions;
using Scribble.Identity.Web.Models.Account;

namespace Scribble.Identity.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize(AuthenticationSchemes = AuthenticationData.AuthenticationSchemes)]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ClaimsManager<ApplicationUser> _claimsManager;
    private readonly IMapper _mapper;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IMapper mapper, ClaimsManager<ApplicationUser> claimsManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _mapper = mapper;
        _claimsManager = claimsManager;
    }

    [AllowAnonymous]
    [HttpGet("~/connect/signin")]
    public IActionResult SignIn(string returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [AllowAnonymous, ValidateAntiForgeryToken]
    [HttpPost("~/connect/signin")]
    public async Task<IActionResult> SignIn(SignInViewModel model, string returnUrl)
    {
        if (!ModelState.IsValid) return View(model);
        
        var signInResult = await _signInManager
            .PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false)
            .ConfigureAwait(false);

        if (signInResult.Succeeded)
        {
            var principal = await _claimsManager.GetPrincipalByEmailAsync(model.Email)
                .ConfigureAwait(false);
           
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal)
                .ConfigureAwait(false);
            
            return Redirect(returnUrl);
        }
        
        ViewData["ReturnUrl"] = returnUrl;
        ModelState.AddSignInErrors(signInResult);

        return View(model);
    }

    [AllowAnonymous]
    [HttpGet("~/connect/signup")]
    public IActionResult SignUp(string returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [AllowAnonymous, ValidateAntiForgeryToken]
    [HttpPost("~/connect/signup")]
    public async Task<IActionResult> SignUp(SignUpViewModel model, string returnUrl)
    {
        if (!ModelState.IsValid) return View(model);
        
        var result = await _userManager
            .CreateAsync(_mapper.Map<ApplicationUser>(model), model.Password)
            .ConfigureAwait(false);

        if (result.Succeeded)
            return RedirectToAction("SignIn", "Account", new
            {
                ReturnUrl = returnUrl
            });

        ModelState.AddIdentityErrors(result.Errors);
        
        return View(model);
    }

    [ValidateAntiForgeryToken]
    [HttpPost("~/connect/signout")]
    public async Task<IActionResult> SignOut(string returnUrl)
    {
        await _signInManager.SignOutAsync()
            .ConfigureAwait(false);

        return this.RedirectToLocal(returnUrl);
    }
}