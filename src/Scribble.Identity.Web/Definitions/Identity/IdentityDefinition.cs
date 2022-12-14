using Calabonga.AspNetCore.AppDefinitions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Models;
using Scribble.Identity.Web.Definitions.Identity.Authorization;
using Scribble.Identity.Web.Definitions.Identity.Policy;

namespace Scribble.Identity.Web.Definitions.Identity;

public class IdentityDefinition : AppDefinition
{
    public override void ConfigureServices(IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/connect/signin";
            })
            .AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? string.Empty;
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? string.Empty;
            })
            .AddVkontakte(options =>
            {
                options.ClientId = builder.Configuration["Authentication:Vkontakte:ClientId"] ?? string.Empty;
                options.ClientSecret = builder.Configuration["Authentication:Vkontakte:ClientSecret"] ?? string.Empty;

                options.AuthorizationEndpoint = builder.Configuration["Authentication:Vkontakte:AuthorizationEndpoint"] ?? string.Empty;
                options.TokenEndpoint = builder.Configuration["Authentication:Vkontakte:TokenEndpoint"] ?? string.Empty;
                
                options.Scope.Add("email");
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Permissions.Users.View, policyBuilder =>
            {
                policyBuilder.AddRequirements(new PermissionRequirement(Permissions.Users.View))
                    .RequireRole(DefaultRoles.Administrator.ToString(), DefaultRoles.Moderator.ToString());
            });
            
            options.AddPolicy(Permissions.Users.Create, policyBuilder =>
            {
                policyBuilder.AddRequirements(new PermissionRequirement(Permissions.Users.Create))
                    .RequireRole(DefaultRoles.Administrator.ToString(), DefaultRoles.Moderator.ToString());
            });
            
            options.AddPolicy(Permissions.Users.Edit, policyBuilder =>
            {
                policyBuilder.AddRequirements(new PermissionRequirement(Permissions.Users.Edit))
                    .RequireRole(DefaultRoles.Administrator.ToString(), DefaultRoles.Moderator.ToString());
            });
            
            options.AddPolicy(Permissions.Users.Delete, policyBuilder =>
            {
                policyBuilder.AddRequirements(new PermissionRequirement(Permissions.Users.Delete))
                    .RequireRole(DefaultRoles.Administrator.ToString());
            });
        });

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>(factory =>
        {
            var userManager = factory.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = factory.GetRequiredService<RoleManager<ApplicationRole>>();

            return new PermissionHandler(userManager, roleManager);
        });
    }

    public override void ConfigureApplication(WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors("CorsPolicy");
        app.UseAuthentication();
        app.UseAuthorization();
    }
}