using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Scribble.Identity.Infrastructure.Contexts;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost;Database=Scribble.Identity;User Id=sa;Password=du85txss10;TrustServerCertificate=True;Trusted_Connection=True;");
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}