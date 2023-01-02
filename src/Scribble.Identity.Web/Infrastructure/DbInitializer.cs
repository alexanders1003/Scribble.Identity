using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Infrastructure.Contexts;
using Scribble.Identity.Models;
using Scribble.Identity.Web.Definitions.Identity.Authorization;

namespace Scribble.Identity.Web.Infrastructure;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            await context.Database.EnsureCreatedAsync();
            
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
            await SeedDefaultRolesAsync(roleManager);

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            await SeedDefaultUsersAsync(userManager);
            
            await context.SaveChangesAsync(CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            var logger = services.GetRequiredService<ILogger>();
            logger.LogError(exception, "An error occurred while seeding the database");
        }
    }

    private static async Task SeedDefaultRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        foreach (var defaultRole in Enum.GetValues<DefaultRoles>())
        {
            var role = defaultRole.ToString();
            
            if (!roleManager.Roles.Any(i => i.Name == role))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = role,
                    NormalizedName = role.ToUpper()
                });
            }

            switch (defaultRole)
            {
                case DefaultRoles.Administrator:
                    var administrator = await roleManager.FindByNameAsync(role)
                        .ConfigureAwait(false);
                    
                    await roleManager.AddClaimAsync(administrator!, new Claim(PermissionClaimTypes.Permission, Permissions.Users.View));
                    await roleManager.AddClaimAsync(administrator!, new Claim(PermissionClaimTypes.Permission, Permissions.Users.Create));
                    await roleManager.AddClaimAsync(administrator!, new Claim(PermissionClaimTypes.Permission, Permissions.Users.Edit));
                    await roleManager.AddClaimAsync(administrator!, new Claim(PermissionClaimTypes.Permission, Permissions.Users.Delete));
                    break;
                case DefaultRoles.Moderator:
                    var moderator = await roleManager.FindByNameAsync(role)
                        .ConfigureAwait(false);
                    
                    await roleManager.AddClaimAsync(moderator!, new Claim(PermissionClaimTypes.Permission, Permissions.Users.View));
                    await roleManager.AddClaimAsync(moderator!, new Claim(PermissionClaimTypes.Permission, Permissions.Users.Create));
                    await roleManager.AddClaimAsync(moderator!, new Claim(PermissionClaimTypes.Permission, Permissions.Users.Edit));
                    break;
                case DefaultRoles.User:
                    var user = await roleManager.FindByNameAsync(role)
                        .ConfigureAwait(false);
                    
                    await roleManager.AddClaimAsync(user!, new Claim(PermissionClaimTypes.Permission, Permissions.Users.View));
                    break;
            }
        }
    }

    private static async Task SeedDefaultUsersAsync(UserManager<ApplicationUser> userManager)
    {
        var defaultUser = new ApplicationUser
        {
            UserName = "alexander.sentsov03@gmail.com",
            Email = "alexander.sentsov03@gmail.com",
            EmailConfirmed = true,
        };

        if (userManager.Users.All(i => i.Id != defaultUser.Id))
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email)
                .ConfigureAwait(false);

            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "!QwERty%10")
                    .ConfigureAwait(false);
                
                await userManager.AddToRolesAsync(defaultUser, Enum.GetValues<DefaultRoles>()
                        .Select(x => x.ToString()))
                    .ConfigureAwait(false);
            }
        }
    }
}