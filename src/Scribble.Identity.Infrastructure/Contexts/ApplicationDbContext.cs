using Microsoft.EntityFrameworkCore;
using Scribble.Identity.Infrastructure.Base;
using Scribble.Identity.Models;

namespace Scribble.Identity.Infrastructure.Contexts;

public class ApplicationDbContext : DbContextBase
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options) { }
    
    public DbSet<EventItem> EventItems { get; set; }
    public DbSet<ApplicationUserProfile> Profiles { get; set; }
    public DbSet<ApplicationPermission> Permissions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.UseOpenIddict<Guid>();
        base.OnModelCreating(builder);
    }
}