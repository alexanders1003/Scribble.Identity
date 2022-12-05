using Calabonga.AspNetCore.AppDefinitions;
using OpenIddict.Abstractions;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Infrastructure.Contexts;
using Scribble.Identity.Web.HostedServices;

namespace Scribble.Identity.Web.Definitions.OpenIddict;

public class OpenIddictDefinition : AppDefinition
{
    public override void ConfigureServices(IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<ApplicationDbContext>()
                    .ReplaceDefaultEntities<Guid>();
            })
            .AddServer(options =>
            {
                options
                    .AllowAuthorizationCodeFlow()
                    .AllowPasswordFlow()
                    .AllowClientCredentialsFlow()
                    .AllowRefreshTokenFlow();
                
                options
                    .SetAuthorizationEndpointUris("/connect/authorize")
                    .SetLogoutEndpointUris("/connect/logout")
                    .SetTokenEndpointUris("/connect/token")
                    .SetUserinfoEndpointUris("/connect/userinfo");
                
                options
                    .AddEphemeralEncryptionKey()
                    .AddEphemeralSigningKey()
                    .DisableAccessTokenEncryption();
                
                options.RegisterScopes(
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles,
                    "api");
                
                options
                    .AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                options
                    .UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableLogoutEndpointPassthrough()
                    .EnableTokenEndpointPassthrough();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        
        services.AddHostedService<OpenIddictWorker>();
    }
}