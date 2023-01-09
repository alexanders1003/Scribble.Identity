using System.Security.Claims;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Web.Controllers;
using Scribble.Identity.Web.Models.Account;

namespace Scribble.Identity.Tests;

public class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute()
        : base(() =>
        {
            var fixture = new Fixture { OmitAutoProperties = true }
                .Customize(new AutoMoqCustomization { ConfigureMembers = false });
            
            fixture.Customize<SignInViewModel>(composer => composer.WithAutoProperties());
            fixture.Customize<ApplicationUser>(composer => composer.WithAutoProperties());

            var mockAuthenticationService = new Mock<IAuthenticationService>();
            mockAuthenticationService
                .Setup(m => m.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.FromResult((object)null!));
            fixture.Register(() => mockAuthenticationService.Object);
            
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(m => m.GetService(typeof(IAuthenticationService)))
                .Returns(mockAuthenticationService.Object);
            fixture.Register((() => mockServiceProvider.Object));
            
            fixture.Register(() => 
                Fake.CreateUserManager<ApplicationUser>().Object);
            
            return fixture;
        })
    {
        
    }
}