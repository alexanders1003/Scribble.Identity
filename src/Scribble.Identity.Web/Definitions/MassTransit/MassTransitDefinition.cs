using Calabonga.AspNetCore.AppDefinitions;
using MassTransit;

namespace Scribble.Identity.Web.Definitions.MassTransit;

public class MassTransitDefinition : AppDefinition
{
    public override void ConfigureServices(IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddMassTransit(x => x.UsingRabbitMq());
        
        services.AddOptions<MassTransitHostOptions>()
            .Configure(options =>
            {
                options.WaitUntilStarted = true;
                options.StartTimeout = TimeSpan.FromSeconds(10);
                options.StopTimeout = TimeSpan.FromSeconds(30);
            });
    }
}