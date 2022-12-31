using OpenIddict.Abstractions;
using Scribble.Identity.Infrastructure.Contexts;

namespace Scribble.Identity.Web;

public class OpenIddictWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public OpenIddictWorker(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);
        
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        if (await manager.FindByClientIdAsync("client-id-code", cancellationToken) is null)
        {
            var url = _serviceProvider.GetRequiredService<IConfiguration>().GetValue<string>("AuthServer:Url");

            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "client-id-code",
                ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
                ClientSecret = "client-secret-code",
                DisplayName = "Scribble Identity Authorization Code Flow",
                RedirectUris =
                {
                    new Uri("https://oauth.pstmn.io/v1/browser-callback"),
                    new Uri("https://oauth.pstmn.io/v1/callback"),
                    new Uri("https://www.getpostman.com/oauth2/callback"),
                    new Uri($"{url}/swagger/oauth2-redirect.html"),                     
                },
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,

                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,

                    OpenIddictConstants.Permissions.Prefixes.Scope + "api",

                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    
                    OpenIddictConstants.ResponseModes.Query
                }
            }, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}