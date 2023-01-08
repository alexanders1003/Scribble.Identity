using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Scribble.Identity.Infrastructure.Contexts;

public class ApplicationUserStore : UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>
{
    public ApplicationUserStore(ApplicationDbContext context, IdentityErrorDescriber? describer = null) 
        : base(context, describer) { }
}