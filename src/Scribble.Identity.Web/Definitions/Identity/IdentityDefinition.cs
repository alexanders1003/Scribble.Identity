using System.Security.Claims;
using Calabonga.AspNetCore.AppDefinitions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
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
                options.ClientId = "916366519754-mfnvns2e48p16lhqg8febs70lfhv7396.apps.googleusercontent.com";
                options.ClientSecret = "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";
            })
            .AddVkontakte(options =>
            {
                options.ClientId = "51521437";
                options.ClientSecret = "aH01GKJxWGQyf58GrC9b";

                options.AuthorizationEndpoint = "https://oauth.vk.com/authorize";
                options.TokenEndpoint = "https://oauth.vk.com/access_token";
                
                options.Scope.Add("email");
            });
            /*.AddOAuth("VK", "Vkontakte", options =>
            {
                options.ClientId = "aH01GKJxWGQyf58GrC9b";
                options.ClientSecret = "1c59e0d61c59e0d61c59e0d6c11f4bc74b11c591c59e0d67fe2e189689b5048dc667bdc";
                options.ClaimsIssuer = "Vkontakte";
                options.CallbackPath = new PathString("/connect/external-signin-callback");
                options.AuthorizationEndpoint = "https://oauth.vk.com/authorize";
                options.TokenEndpoint = "https://oauth.vk.com/access_token";
                options.Scope.Add("email");
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "user_id");
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                options.SaveTokens = true;
                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = context =>
                    {
                        context.RunClaimActions(context.TokenResponse.Response!.RootElement);
                        return Task.CompletedTask;
                    },
                    OnRemoteFailure = _ => Task.CompletedTask
                };
            });*/

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