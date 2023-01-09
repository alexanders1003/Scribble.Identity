using System.Security.Claims;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit.Text;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Web.Definitions.OpenIddict;
using Scribble.Identity.Web.Infrastructure.Extensions;
using Scribble.Identity.Web.Models.Account;
using Scribble.Mail.Contracts;

namespace Scribble.Identity.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize(AuthenticationSchemes = AuthenticationData.AuthenticationSchemes)]
public class AccountController : Controller
{
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ClaimsManager<ApplicationUser> _claimsManager;
    private readonly IPublishEndpoint _publishEndpoint;

    public AccountController(IMapper mapper, UserManager<ApplicationUser> userManager, 
        SignInManager<ApplicationUser> signInManager, ClaimsManager<ApplicationUser> claimsManager,
        IPublishEndpoint publishEndpoint)
    {
        _mapper = mapper;
        _userManager = userManager;
        _signInManager = signInManager;
        _claimsManager = claimsManager;
        _publishEndpoint = publishEndpoint;
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
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        
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
    [HttpPost("~/connect/external-signin")]
    public IActionResult ExternalSignIn(string provider, string returnUrl)
    {
        var redirectUrl = Url.Action("ExternalSignInCallback", "Account",
            new { returnUrl });

        var properties = _signInManager
            .ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        return new ChallengeResult(provider, properties);
    }

    [AllowAnonymous]
    [HttpGet("~/connect/external-signin-callback")]
    public async Task<IActionResult> ExternalSignInCallback(string? returnUrl = null, string? remoteError = null)
    {
        returnUrl ??= Url.Content("~/");

        if (remoteError != null)
        {
            ModelState.AddModelError("ExternalError", $"Error from external provider: {remoteError}");
            ViewData["ReturnUrl"] = returnUrl;
            return View("SignIn");
        }

        var info = await _signInManager
            .GetExternalLoginInfoAsync()
            .ConfigureAwait(false);

        if (info == null)
        {
            ModelState.AddModelError("ExternalError", $"Error loading external sign in information.");
            ViewData["ReturnUrl"] = returnUrl;
            return View("SignIn");
        }

        var result = await _signInManager
            .ExternalLoginSignInAsync(info.LoginProvider,
                info.ProviderKey, isPersistent: false, bypassTwoFactor: true)
            .ConfigureAwait(false);

        var email = info.Principal
            .FindFirstValue(ClaimTypes.Email);
        
        if (result.Succeeded)
        {
            var principal = await _claimsManager.GetPrincipalByEmailAsync(email)
                .ConfigureAwait(false);
           
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal)
                .ConfigureAwait(false);
            
            return Redirect(returnUrl);
        }

        if (email != null)
        {
            var user = await _userManager
                .FindByEmailAsync(email)
                .ConfigureAwait(false);

            if (user == null)
            {
                user = new ApplicationUser { UserName = email, Email = email };
               
                await _userManager
                    .CreateAsync(user)
                    .ConfigureAwait(false);
            }

            await _userManager
                .AddLoginAsync(user, info)
                .ConfigureAwait(false);
            await _signInManager
                .SignInAsync(user, isPersistent: false);
            
            var principal = await _claimsManager.GetPrincipalByEmailAsync(email)
                .ConfigureAwait(false);
           
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal)
                .ConfigureAwait(false);

            return Redirect(returnUrl);
        }

        return View("Error");
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
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        var result = await _userManager
            .CreateAsync(_mapper.Map<ApplicationUser>(model), model.Password)
            .ConfigureAwait(false);

        if (result.Succeeded)
        {
            var user = await _userManager
                .FindByEmailAsync(model.Email)
                .ConfigureAwait(false);

            var code = await _userManager
                .GenerateEmailConfirmationTokenAsync(user!)
                .ConfigureAwait(false);
            
            var callbackUrl = Url
                .Action("ConfirmEmail", 
                    "Account", 
                    new { userId = user!.Id, code }, protocol: HttpContext.Request.Scheme);

            await _publishEndpoint.Publish<MailMessageRequestContract>(new()
            {
                Recipient = user.Email!,
                Subject = "Verify your email address",
                Message = $"<a href=\"{callbackUrl}\">Verify</a>",
                Format = TextFormat.Html
            });

            ViewData["ReturnUrl"] = returnUrl;
            return View("SignUpСompletion");
        }

        ModelState.AddIdentityErrors(result.Errors);

        ViewData["ReturnUrl"] = returnUrl;
        return View(model);
    }

    [ValidateAntiForgeryToken]
    [HttpPost("~/connect/signout")]
    public async Task<IActionResult> SignOut(string returnUrl)
    {
        await _signInManager.SignOutAsync()
            .ConfigureAwait(false);

        return RedirectToAction("SignIn", "Account", new
        {
            ReturnUrl = returnUrl
        });
    }

    [AllowAnonymous]
    [HttpGet("~/connect/confirm")]
    public async Task<IActionResult> ConfirmEmail(string? userId, string? code)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            return View("Error");
        
        var user = await _userManager
            .FindByIdAsync(userId)
            .ConfigureAwait(false);

        if (user == null)
            return View("Error");
        
        var result = await _userManager
            .ConfirmEmailAsync(user, code)
            .ConfigureAwait(false);

        if (result.Succeeded)
            return View("ConfirmEmail");
        return View("Error");
    }

    [AllowAnonymous]
    [HttpGet("~/connect/challenge")]
    public IActionResult ForgotPassword(string returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("~/connect/challenge")]
    [AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, string returnUrl)
    {
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email)
            .ConfigureAwait(false);

        if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            return View("Error");

        var code = await _userManager.GeneratePasswordResetTokenAsync(user)
            .ConfigureAwait(false);
        var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code, returnUrl },
            protocol: HttpContext.Request.Scheme);

        await _publishEndpoint.Publish<MailMessageRequestContract>(new()
        {
            Recipient = user.Email!,
            Subject = "Reset password - Scribble Accounts",
            Message = $"<a href=\"{callbackUrl}\">Reset</a>",
            Format = TextFormat.Html
        });

        ViewData["ReturnUrl"] = returnUrl;
        return View("ForgotPasswordConfirmation");
    }

    [AllowAnonymous]
    [HttpGet("~/connect/reset")]
    public IActionResult ResetPassword(string userId, string code, string returnUrl)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            return View("Error");

        ViewData["UserId"] = userId;
        ViewData["Code"] = code;
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [AllowAnonymous, ValidateAntiForgeryToken]
    [HttpPost("~/connect/reset")]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, string userId, string code, string returnUrl)
    {
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(userId)
            .ConfigureAwait(false);

        if (user == null)
            return View("Error");

        var result = await _userManager.ResetPasswordAsync(user, code, model.Password)
            .ConfigureAwait(false);

        if (result.Succeeded)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View("ResetPasswordConfirmation");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(model);
    }
}