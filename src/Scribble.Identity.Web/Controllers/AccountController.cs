using AutoMapper;
using MassTransit;
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
    private readonly IPublishEndpoint _publishEndpoint;

    public AccountController(IMapper mapper, UserManager<ApplicationUser> userManager, 
        SignInManager<ApplicationUser> signInManager, IPublishEndpoint publishEndpoint)
    {
        _mapper = mapper;
        _userManager = userManager;
        _signInManager = signInManager;
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
            return Redirect(returnUrl);

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
            
            return RedirectToAction("SignIn", "Account", new
            {
                ReturnUrl = returnUrl
            });
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

        return this.RedirectToLocal(returnUrl);
    }

    [AllowAnonymous]
    [HttpGet("~/connect/confirm")]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        var user = await _userManager
            .FindByIdAsync(userId)
            .ConfigureAwait(false);
        
        var result = await _userManager
            .ConfirmEmailAsync(user!, code)
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

    public Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, string returnUrl)
    {
        throw new NotImplementedException();
    }

    [AllowAnonymous]
    [HttpGet("~/connect/reset")]
    public IActionResult ResetPassword(string code, string returnUrl)
    {
        throw new NotImplementedException();
    }

    [AllowAnonymous, ValidateAntiForgeryToken]
    [HttpPost("~/connect/reset")]
    public Task<IActionResult> ResetPassword(ResetPasswordViewModel model, string returnUrl)
    {
        throw new NotImplementedException();
    }
}