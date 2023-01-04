using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Scribble.Identity.Infrastructure;

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

    public static Mock<SignInManager<TUser>> CreateSignInManager<TUser>() where TUser : class
    {
        var mockUserManager = CreateUserManager<TUser>();

        return new Mock<SignInManager<TUser>>(
            mockUserManager.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<TUser>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<ILogger<SignInManager<TUser>>>().Object,
            new Mock<IAuthenticationSchemeProvider>().Object,
            new Mock<IUserConfirmation<TUser>>().Object);
    }

    public static Mock<IUrlHelper> CreateUrlHelper()
    {
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper
            .Setup(m => m.Action(It.IsAny<UrlActionContext>()))
            .Returns(It.IsAny<string>())
            .Verifiable();

        return mockUrlHelper;
    }

    public static Mock<IPublishEndpoint> CreatePublishEndpoint<T>() where T : class
    {
        var mockPublishEndpoint = new Mock<IPublishEndpoint>();
        mockPublishEndpoint
            .Setup(m => m.Publish(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return mockPublishEndpoint;
    }
}