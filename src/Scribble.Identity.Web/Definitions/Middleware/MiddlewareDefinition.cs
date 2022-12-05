using AutoWrapper;
using Calabonga.AspNetCore.AppDefinitions;

namespace Scribble.Identity.Web.Definitions.Middleware;

public class MiddlewareDefinition : AppDefinition
{
    public override void ConfigureApplication(WebApplication app)
    {
        app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions
        {
            ApiVersion = "1.0.0.0",
            IgnoreNullValue = true,
            IsApiOnly = false
        });
    }
}