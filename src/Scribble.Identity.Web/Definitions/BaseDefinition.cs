using Calabonga.AspNetCore.AppDefinitions;

namespace Scribble.Identity.Web.Definitions;

public class BaseDefinition : AppDefinition
{
    public override void ConfigureServices(IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddLocalization();
        services.AddHttpContextAccessor();
        services.AddResponseCaching();
        services.AddMemoryCache();

        services.AddRouting(options =>
        {
            options.LowercaseUrls = true;
        });
        
        services.AddMvc();
    }

    public override void ConfigureApplication(WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.MapDefaultControllerRoute();
    }
}