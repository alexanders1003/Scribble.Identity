using Calabonga.AspNetCore.AppDefinitions;
using Scribble.Identity.Infrastructure;

namespace Scribble.Identity.Web.Definitions.Managers;

public class ManagersDefinition : AppDefinition
{
    public override void ConfigureServices(IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddTransient<ClaimsManager<ApplicationUser>>();
    }
}