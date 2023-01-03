using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Scribble.Identity.Tests;

public static class Fake
{
    public static Mock<DbSet<T>> CreateDbSet<T>(IQueryable<T> collection) where T : class
    {
        var studDbSet = new Mock<DbSet<T>>();
        studDbSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(collection.Provider);
        studDbSet.As<IQueryable<T>>()
            .Setup(m => m.Expression)
            .Returns(collection.Expression);
        studDbSet.As<IQueryable<T>>()
            .Setup(m => m.ElementType)
            .Returns(collection.ElementType);
        studDbSet.As<IQueryable<T>>()
            .Setup(m => m.GetEnumerator())
            .Returns(collection.GetEnumerator());

        return studDbSet;
    }


    public static Mock<UserManager<TUser>> CreateUserManager<TUser>() where TUser : class
    {
        return new Mock<UserManager<TUser>>(
            new Mock<IUserStore<TUser>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<TUser>>().Object,
            Array.Empty<IUserValidator<TUser>>(),
            Array.Empty<IPasswordValidator<TUser>>(),
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<TUser>>>().Object);
    }
}