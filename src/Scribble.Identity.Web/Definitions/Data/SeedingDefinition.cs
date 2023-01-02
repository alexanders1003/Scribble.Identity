using Calabonga.AspNetCore.AppDefinitions;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Web.Infrastructure;

namespace Scribble.Identity.Web.Definitions.Data;

public class SeedingDefinition : AppDefinition
{
    public override void ConfigureApplication(WebApplication app)
    {
        DbInitializer.InitializeAsync(app.Services).Wait();
    }
}