using System.Security.Claims;
using AutoMapper;
using AutoWrapper.Wrappers;
using Calabonga.Microservices.Core;
using Microsoft.AspNetCore.Identity;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Web.Application.Managers.Base;
using Scribble.Identity.Web.Models.Account;

namespace Scribble.Identity.Web.Application.Managers;

public class AccountManager : IAccountManager
{
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IMapper _mapper;
    private readonly ILogger<AccountManager> _logger;

    public AccountManager(UserManager<ApplicationUser> userManager, 
        RoleManager<ApplicationRole> roleManager, 
        IMapper mapper, 
        ILogger<AccountManager> logger, 
        IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
        _logger = logger;
        _claimsFactory = claimsFactory;
    }

    public async Task<UserViewModel> RegisterAsync(RegisterViewModel model, CancellationToken token)
    {
        var user = _mapper.Map<ApplicationUser>(model);
        
        var result = await _userManager.CreateAsync(user, model.Password)
            .ConfigureAwait(false);

        if (!result.Succeeded) 
            throw new ApiException(result.Errors);
        
        if (!await _roleManager.RoleExistsAsync("User").ConfigureAwait(false))
            throw new ApiException("Role not found.");

        await _userManager.AddToRoleAsync(user, "User")
            .ConfigureAwait(false);

        var principal = await _claimsFactory.CreateAsync(user)
            .ConfigureAwait(false);
            
        _logger.LogInformation("New user was registered. Email {UserEmail}", user.Email);

        var i = _mapper.Map<UserViewModel>(principal.Identity);

        return i;
    }
    
    public async Task<UserViewModel> GetUserByIdAsync(string id, CancellationToken token)
    {
        var user = await _userManager.FindByIdAsync(id)
            .ConfigureAwait(false);

        return _mapper.Map<UserViewModel>(user);
    }

    public async Task<UserViewModel> GetUserByEmailAsync(string email, CancellationToken token)
    {
        var user = await _userManager.FindByEmailAsync(email)
            .ConfigureAwait(false);

        return _mapper.Map<UserViewModel>(user);    
    }

    public IEnumerable<string> GetUserRoles(ClaimsPrincipal principal)
    {
        return ClaimsHelper.GetValues<string>((ClaimsIdentity)principal.Identity!, "role");
    }
}