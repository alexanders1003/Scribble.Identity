using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Scribble.Identity.Infrastructure.Contexts;

public class ApplicationUserStore : UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>
{
    public ApplicationUserStore(ApplicationDbContext context, IdentityErrorDescriber? describer = null) 
        : base(context, describer) { }

    public override async Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken = new())
    {
        return await Users
            .Include(i => i.ApplicationUserProfile).ThenInclude(i => i!.Permissions)
            .FirstOrDefaultAsync(user => user.Id.ToString() == userId, cancellationToken)
            .ConfigureAwait(false);
    }

    public override async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = new CancellationToken())
    {
        return await Users
            .Include(i => i.ApplicationUserProfile).ThenInclude(i => i!.Permissions)
            .FirstOrDefaultAsync(user => user.NormalizedUserName == normalizedUserName, cancellationToken)
            .ConfigureAwait(false);
    }
}