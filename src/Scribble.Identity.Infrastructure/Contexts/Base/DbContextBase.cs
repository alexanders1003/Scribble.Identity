using System.Reflection;
using Calabonga.UnitOfWork;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Scribble.Identity.Models.Base;

namespace Scribble.Identity.Infrastructure.Contexts.Base;

public abstract class DbContextBase : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private const string DefaultUserName = "Anonymous";

    protected DbContextBase() { }
    protected DbContextBase(DbContextOptions options) 
        : base(options) => LastSaveChangesResult = new SaveChangesResult();

    private SaveChangesResult? LastSaveChangesResult { get; }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            DbSaveChanges();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (Exception exception)
        {
            LastSaveChangesResult.Exception = exception;
            return Task.FromResult(0);
        }
    }
    
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        try
        {
            DbSaveChanges();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
        catch (Exception exception)
        {
            LastSaveChangesResult.Exception = exception;
            return 0;
        }
    }
    
    public override int SaveChanges()
    {
        try
        {
            DbSaveChanges();
            return base.SaveChanges();
        }
        catch (Exception exception)
        {
            LastSaveChangesResult.Exception = exception;
            return 0;
        }
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            DbSaveChanges();
            return base.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            LastSaveChangesResult.Exception = exception;
            return Task.FromResult(0);
        }
    }
    
    private void DbSaveChanges()
    {
        var createdEntries = ChangeTracker.Entries().Where(e => e.State == EntityState.Added);
        foreach (var entry in createdEntries)
        {
            if (!(entry.Entity is IAuditable))
            {
                continue;
            }

            var creationDate = DateTime.Now.ToUniversalTime();
            var userName = entry.Property("CreatedBy").CurrentValue == null
                ? DefaultUserName
                : entry.Property("CreatedBy").CurrentValue;
            var updatedAt = entry.Property("UpdatedAt").CurrentValue;
            var createdAt = entry.Property("CreatedAt").CurrentValue;
            if (createdAt != null)
            {
                entry.Property("CreatedAt").CurrentValue = DateTime.Parse(createdAt.ToString() ?? string.Empty).Year > 1970 
                    ? ((DateTime)createdAt).ToUniversalTime() : creationDate;
            }
            else
            {
                entry.Property("CreatedAt").CurrentValue = creationDate;
            }

            if (updatedAt != null)
            {
                entry.Property("UpdatedAt").CurrentValue = DateTime.Parse(updatedAt.ToString() ?? string.Empty).Year > 1970 
                    ? ((DateTime)updatedAt).ToUniversalTime() : creationDate;
            }
            else
            {
                entry.Property("UpdatedAt").CurrentValue = creationDate;
            }

            entry.Property("CreatedBy").CurrentValue = userName;
            entry.Property("UpdatedBy").CurrentValue = userName;

            LastSaveChangesResult.AddMessage($"ChangeTracker has new entities: {entry.Entity.GetType()}");
        }

        var updatedEntries = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified);
        foreach (var entry in updatedEntries)
        {
            if (entry.Entity is IAuditable)
            {
                var userName = entry.Property("UpdatedBy").CurrentValue == null
                    ? DefaultUserName
                    : entry.Property("UpdatedBy").CurrentValue;
                entry.Property("UpdatedAt").CurrentValue = DateTime.Now.ToUniversalTime();
                entry.Property("UpdatedBy").CurrentValue = userName;
            }

            LastSaveChangesResult.AddMessage($"ChangeTracker has modified entities: {entry.Entity.GetType()}");
        }
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var applyGenericMethod = typeof(ModelBuilder).GetMethods(BindingFlags.Instance | BindingFlags.Public).First(x => x.Name == "ApplyConfiguration");
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(c => c.IsClass && !c.IsAbstract && !c.ContainsGenericParameters))
        {
            foreach (var item in type.GetInterfaces())
            {
                if (!item.IsConstructedGenericType || item.GetGenericTypeDefinition() != typeof(IEntityTypeConfiguration<>))
                {
                    continue;
                }

                var applyConcreteMethod = applyGenericMethod.MakeGenericMethod(item.GenericTypeArguments[0]);
                applyConcreteMethod.Invoke(builder, new[] { Activator.CreateInstance(type) });
                break;
            }
        }
    }
}