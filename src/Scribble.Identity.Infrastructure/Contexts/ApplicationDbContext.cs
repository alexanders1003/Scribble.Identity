using Microsoft.EntityFrameworkCore;
using Scribble.Identity.Infrastructure.Contexts.Base;
using Scribble.Identity.Models;

namespace Scribble.Identity.Infrastructure.Contexts;

public class ApplicationDbContext : DbContextBase
{
    public ApplicationDbContext() { }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options) { }

    public DbSet<EventItem> EventItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.UseOpenIddict<Guid>();
        base.OnModelCreating(builder);
    }
}