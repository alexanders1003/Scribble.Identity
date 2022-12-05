using Calabonga.AspNetCore.AppDefinitions;
using Scribble.Identity.Models.Base;

namespace Scribble.Identity.Web.Definitions.Cors;

public class CorsDefinition : AppDefinition
{
    public override void ConfigureServices(IServiceCollection services, WebApplicationBuilder builder)
    {
        var origins = builder.Configuration
            .GetSection("Cors")?
            .GetSection("Origins")?
            .Value?.Split(",");

        services.AddCors(options =>
        {
            options.AddPolicy(AppData.PolicyName, policyBuilder =>
            {
                policyBuilder.AllowAnyHeader();
                policyBuilder.AllowAnyMethod();

                if (origins is not { Length: > 0 }) return;

                if (origins.Contains("*"))
                {
                    policyBuilder.AllowAnyHeader();
                    policyBuilder.AllowAnyMethod();
                    policyBuilder.SetIsOriginAllowed(host => true);
                    policyBuilder.AllowCredentials();
                }
                else
                {
                    foreach (var origin in origins)
                    {
                        policyBuilder.WithOrigins(origin);
                    }
                }
            });
        });
    }
}