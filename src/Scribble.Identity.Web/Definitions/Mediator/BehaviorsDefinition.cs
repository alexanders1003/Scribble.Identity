using Calabonga.AspNetCore.AppDefinitions;
using MediatR;
using Scribble.Identity.Web.Application;
using Scribble.Identity.Web.Infrastructure.Behaviors;

namespace Scribble.Identity.Web.Definitions.Mediator;

public class BehaviorsDefinition : AppDefinition
{
    public override void ConfigureServices(IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    }
}