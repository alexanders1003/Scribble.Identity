using Calabonga.AspNetCore.AppDefinitions;
using Scribble.Identity.Web.Application.Managers;
using Scribble.Identity.Web.Application.Managers.Base;

namespace Scribble.Identity.Web.Definitions.Managers;

public class ManagersDefinition : AppDefinition
{
    public override void ConfigureServices(IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddTransient<IAccountManager, AccountManager>();
        services.AddTransient<IClaimsManager, ClaimsManager>();
    }
}