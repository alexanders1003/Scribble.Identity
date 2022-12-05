using Calabonga.AspNetCore.AppDefinitions;
using MediatR;

namespace Scribble.Identity.Web.Definitions.Mediator;

public class MediatorDefinition : AppDefinition
{
    public override void ConfigureServices(IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddMediatR(typeof(Program));
    }
}