using Calabonga.AspNetCore.AppDefinitions;
using Microsoft.OpenApi.Models;
using Scribble.Identity.Models.Base;
using Scribble.Identity.Web.Infrastructure.Attributes;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Scribble.Identity.Web.Definitions.Swagger;

public class SwaggerDefinition : AppDefinition
{
    private const string AppVersion = $"{ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}";
    private const string SwaggerConfig = "/swagger/v1/swagger.json";
    
    public override void ConfigureApplication(WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        var url =  app.Services.GetRequiredService<IConfiguration>().GetValue<string>("Authentication:ServerUrl");

        app.UseSwagger();
        app.UseSwaggerUI(settings =>
        {
            settings.SwaggerEndpoint(SwaggerConfig, $"{AppData.ServiceName} v.{AppVersion}");
            settings.HeadContent = $"{ThisAssembly.Git.Branch.ToUpper()} {ThisAssembly.Git.Commit.ToUpper()}";
            settings.DocumentTitle = $"{AppData.ServiceName}";
            settings.DefaultModelExpandDepth(0);
            settings.DefaultModelRendering(ModelRendering.Model);
            settings.DefaultModelsExpandDepth(0);
            settings.DocExpansion(DocExpansion.None);
            settings.OAuthScopeSeparator(" ");
            settings.OAuthClientId("client-id-code");
            settings.OAuthClientSecret("client-secret-code");
            settings.DisplayRequestDuration();
            settings.OAuthAppName(AppData.ServiceName);
            settings.OAuth2RedirectUrl($"{url}/swagger/oauth2-redirect.html");
        });
    }

    public override void ConfigureServices(IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = AppData.ServiceName,
                Version = AppVersion,
            });

            options.ResolveConflictingActions(x => x.First());

            options.TagActionsBy(api =>
            {
                if (api.ActionDescriptor is not { } descriptor)
                    return !string.IsNullOrEmpty(api.RelativePath)
                        ? new List<string> { api.RelativePath }
                        : new List<string>();
                
                var attribute = descriptor.EndpointMetadata.OfType<FeatureGroupNameAttribute>().FirstOrDefault();
                    
                return new List<string>
                {
                    attribute?.GroupName ?? descriptor.RouteValues["controller"] ?? "Untitled"
                };
            });

            var url = builder.Configuration.GetSection("Authentication").GetValue<string>("ServerUrl");

            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        TokenUrl = new Uri($"{url}/connect/token", UriKind.Absolute),
                        AuthorizationUrl = new Uri($"{url}/connect/authorize", UriKind.Absolute),
                        Scopes = new Dictionary<string, string>
                        {
                            { "api", "Default scope" }
                        }
                    }
                },
                
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        },
                        In = ParameterLocation.Cookie,
                        Type = SecuritySchemeType.OAuth2
                    },
                    new List<string>()
                }
            });
        });
    }
}