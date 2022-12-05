using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scribble.Identity.Infrastructure.Contexts;
using Scribble.Identity.Infrastructure.Exceptions;
using Scribble.Identity.Models.Base;

namespace Scribble.Identity.Infrastructure;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var services = scope.ServiceProvider;
            
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();

                var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

                await SeedRolesAsync(context, roleManager);

                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

                await SeedUsersAsync(context, userManager);

                await context.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                var logger = services.GetRequiredService<ILogger>();
                logger.LogError(exception, "An error occurred while seeding the database");
            }
        }
    }
    
    private static async Task SeedRolesAsync(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
    {
        foreach (var role in AppData.Roles)
        {
            if (!context.Roles.Any(i => i.Name == role))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = role,
                    NormalizedName = role.ToUpper()
                });
            }
        }
    }

    private static async Task SeedUsersAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        var developer = new ApplicationUser
        {
            Email = "alexander.sentsov03@gmail.com",
            UserName = "Developer",
            FirstName = "Alexander",
            LastName = "Sentsov",
            PhoneNumber = "+79923028603",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            ApplicationUserProfile = new ApplicationUserProfile
            {
                CreatedAt = DateTime.Now,
                CreatedBy = "DbInitializer",
                Permissions = new List<ApplicationPermission>
                {
                    new()
                    {
                        CreatedAt = DateTime.Now,
                        CreatedBy = "DbInitializer",
                        PolicyName = "EventItems:UserRoles:View",
                        Description = "Access policy for EventItems controller user view"
                    }
                }
            }
        };

        if (context.Users.Any(i => i.UserName == developer.UserName))
        {
            var result = await userManager.CreateAsync(developer, "du85txSS10%0")
                .ConfigureAwait(false);

            if (!result.Succeeded)
                throw new MicroserviceIdentityValidationException(errors: result.Errors);

            result = await userManager.AddToRolesAsync(developer, AppData.Roles)
                .ConfigureAwait(false);
        
            if (!result.Succeeded)
                throw new MicroserviceIdentityValidationException(result.Errors);
        }
    }
}