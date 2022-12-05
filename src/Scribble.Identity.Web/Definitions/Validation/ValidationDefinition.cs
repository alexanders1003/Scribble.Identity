using Calabonga.AspNetCore.AppDefinitions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Scribble.Identity.Web.Definitions.Validation;

public class ValidationDefinition : AppDefinition
{
    public override void ConfigureServices(IServiceCollection services, WebApplicationBuilder builder)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        services.AddValidatorsFromAssembly(typeof(Program).Assembly);
    }
}