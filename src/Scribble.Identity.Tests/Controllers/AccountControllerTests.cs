using System.Security.Claims;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Web.Controllers;
using Scribble.Identity.Web.Definitions.Mapping;
using Scribble.Identity.Web.Models.Account;
using Scribble.Mail.Contracts;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Scribble.Identity.Tests.Controllers;

public class AccountControllerTests
{
    private readonly IMapper _mapper;
    public AccountControllerTests()
    {
        var mapperConfig = new MapperConfiguration(configurator =>
        {
            configurator.AddProfile<AccountMappingConfiguration>();
        });
        
        _mapper = mapperConfig.CreateMapper();
    }

    [Theory, AutoMoqData]
    public void SignIn_Get_ShouldReturnViewWithReturnUrl_ViewResult([NoAutoProperties] AccountController controller)
    {
        var result = controller.SignIn(It.IsAny<string>());

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewData.ContainsKey("ReturnUrl"));
    }
    
    [Theory, AutoMoqData]
    public async Task SignIn_Post_ShouldRedirectToReturnUrlBecauseModelIsValid_RedirectResult(
        [Frozen] Mock<SignInManager<ApplicationUser>> mockSignInManager,
        [Frozen] Mock<ClaimsManager<ApplicationUser>> mockClaimsManager,
        ApplicationUser user, SignInViewModel model)
    {

        mockSignInManager
            .Setup(m => m.PasswordSignInAsync(model.Email, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Success);

        mockClaimsManager
            .Setup(m => m.GetPrincipalByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new("name", "test name")
            })));

        var controller = new AccountController(
            It.IsAny<IMapper>(),
            It.IsAny<UserManager<ApplicationUser>>(),
            mockSignInManager.Object,
            mockClaimsManager.Object,
            It.IsAny<IPublishEndpoint>());

        var result = await controller.SignIn(model, "https://localhost:5001");

        Assert.IsType<RedirectResult>(result);
    }
    
    [Fact]
    public async Task SignIn_Post_ShouldReturnViewWithReturnUrlBecauseModelIsInvalid_ViewResult()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "example@mail.ru", Email = "example@mail.ru" };
        var model = new SignInViewModel { Email = "example", Password = "password", RememberMe = false };

        var mockUserManager = Fake.CreateUserManager<ApplicationUser>();
        mockUserManager
            .Setup(m => m.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(user);

        var mockSignInManager = Fake.CreateSignInManager<ApplicationUser>();
        mockSignInManager
            .Setup(m => m.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false))
            .Returns(Task.FromResult(SignInResult.Success));
        
        var mockClaimsManager = new Mock<ClaimsManager<ApplicationUser>>();
        mockClaimsManager
            .Setup(m => m.GetPrincipalByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<ClaimsPrincipal>());

        var controller = new AccountController(_mapper,
            mockUserManager.Object,
            mockSignInManager.Object,
            mockClaimsManager.Object,
            It.IsAny<IPublishEndpoint>());
        
        controller.ModelState.AddModelError("ModelError", "Some error occured while checking the model");

        var result = await controller.SignIn(model, "https://localhost:5001");

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewData.ContainsKey("ReturnUrl"));
    }
    
    [Fact]
    public async Task SignIn_Post_ShouldReturnViewBecauseSignInFailed_ViewResult()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "example@mail.ru", Email = "example@mail.ru" };
        var model = new SignInViewModel { Email = "example@mail.ru", Password = "password", RememberMe = false };

        var mockUserManager = Fake.CreateUserManager<ApplicationUser>();
        mockUserManager
            .Setup(m => m.FindByNameAsync(user.UserName!))
            .Returns(Task.FromResult(user)!);

        var mockSignInManager = Fake.CreateSignInManager<ApplicationUser>();
        mockSignInManager
            .Setup(m => m.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false))
            .Returns(Task.FromResult(SignInResult.Failed));
        
        var mockClaimsManager = new Mock<ClaimsManager<ApplicationUser>>();
        mockClaimsManager
            .Setup(m => m.GetPrincipalByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<ClaimsPrincipal>());

        var controller = new AccountController(_mapper,
            mockUserManager.Object,
            mockSignInManager.Object,
            mockClaimsManager.Object,
            It.IsAny<IPublishEndpoint>());

        var result = await controller.SignIn(model, "https://localhost:5001");

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewData.ContainsKey("ReturnUrl"));
    }

    [Fact]
    public void SignUp_Get_ShouldReturnViewWithReturnUrl_ViewResult()
    {
        var controller = new AccountController(_mapper,
            It.IsAny<UserManager<ApplicationUser>>(),
            It.IsAny<SignInManager<ApplicationUser>>(),
            It.IsAny<ClaimsManager<ApplicationUser>>(),
        It.IsAny<IPublishEndpoint>());

        var result = controller.SignUp(It.IsAny<string>());

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewData.ContainsKey("ReturnUrl"));
    }

    [Fact]
    public async Task SignUp_Post_ShouldRedirectToActionWithReturnUrlBecauseModelIsValid_RedirectToActionResult()
    {
        var users = new List<ApplicationUser>  
        {
            new() { Email = "example1@mail.ru" },
            new() { Email = "example2@mail.ru" }
        };
        
        var user = new ApplicationUser { UserName = "example3@mail.ru", Email = "example3@mail.ru" };
        
        var model = new SignUpViewModel
        {
            Email = "example3@mail.ru", 
            Password = "password", 
            ConfirmPassword = "password"
        };

        var mockUserManager = Fake.CreateUserManager<ApplicationUser>();
        mockUserManager
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationUser, string>((x, _) => users.Add(x));
        mockUserManager
            .Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        mockUserManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        mockUserManager
            .Setup(m => m.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync(It.IsAny<string>());

        var controller = new AccountController(_mapper,
            mockUserManager.Object,
            It.IsAny<SignInManager<ApplicationUser>>(),
            It.IsAny<ClaimsManager<ApplicationUser>>(),
            Fake.CreatePublishEndpoint<MailMessageRequestContract>().Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            },
            Url = Fake.CreateUrlHelper().Object
        };

        var result = await controller.SignUp(model, It.IsAny<string>());
        
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.True(redirectToActionResult.RouteValues!.ContainsKey("ReturnUrl"));
        Assert.Equal(3, users.Count);
    }
    
    [Fact]
    public async Task SignUp_Post_ShouldReturnViewWithReturnUrlBecauseModelIsInvalid_ViewResult()
    {
        var user = new ApplicationUser { UserName = "example@mail.ru", Email = "example@mail.ru" };
        
        var model = new SignUpViewModel
        {
            Email = "example", 
            Password = "password", 
            ConfirmPassword = "password"
        };

        var mockUserManager = Fake.CreateUserManager<ApplicationUser>();
        mockUserManager
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        mockUserManager
            .Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        mockUserManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        mockUserManager
            .Setup(m => m.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync(It.IsAny<string>());

        var controller = new AccountController(_mapper,
            mockUserManager.Object,
            It.IsAny<SignInManager<ApplicationUser>>(),
            It.IsAny<ClaimsManager<ApplicationUser>>(),
            Fake.CreatePublishEndpoint<MailMessageRequestContract>().Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            },
            Url = Fake.CreateUrlHelper().Object
        };
        
        controller.ModelState.AddModelError("ModelError", "Some error occured while checking the model");

        var result = await controller.SignUp(model, It.IsAny<string>());
        
        var viewResult = Assert.IsType<ViewResult>(result); 
        Assert.True(viewResult.ViewData.ContainsKey("ReturnUrl"));
    }
    
    [Fact]
    public async Task SignUp_Post_ShouldReturnViewWithReturnUrlBecauseCannotCreateUser_ViewResult()
    {
        var user = new ApplicationUser { UserName = "example@mail.ru", Email = "example@mail.ru" };
        
        var model = new SignUpViewModel
        {
            Email = "example", 
            Password = "password", 
            ConfirmPassword = "password"
        };

        var mockUserManager = Fake.CreateUserManager<ApplicationUser>();
        mockUserManager
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed());
        mockUserManager
            .Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        mockUserManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        mockUserManager
            .Setup(m => m.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync(It.IsAny<string>());

        var controller = new AccountController(_mapper,
            mockUserManager.Object,
            It.IsAny<SignInManager<ApplicationUser>>(),
            It.IsAny<ClaimsManager<ApplicationUser>>(),
            Fake.CreatePublishEndpoint<MailMessageRequestContract>().Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            },
            Url = Fake.CreateUrlHelper().Object
        };
        
        var result = await controller.SignUp(model, It.IsAny<string>());
        
        var viewResult = Assert.IsType<ViewResult>(result); 
        Assert.True(viewResult.ViewData.ContainsKey("ReturnUrl"));
    }

    [Fact]
    public async Task SignOut_Post_ShouldRedirectToSignInActionWithReturnUrl_RedirectToActionResult()
    {
        var mockSignInManager = Fake.CreateSignInManager<ApplicationUser>();
        mockSignInManager
            .Setup(m => m.SignOutAsync());

        var controller = new AccountController(_mapper,
            It.IsAny<UserManager<ApplicationUser>>(),
            mockSignInManager.Object,
            It.IsAny<ClaimsManager<ApplicationUser>>(),
            It.IsAny<IPublishEndpoint>());

        var result = await controller.SignOut(It.IsAny<string>());

        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.True(redirectToActionResult.RouteValues!.ContainsKey("ReturnUrl"));
    }

    [Fact]
    public async Task ConfirmEmail_Get_ShouldReturnConfirmEmailViewBecauseConfirmEmailWasSucceeded_ViewResult()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "example@mail.ru" };
        
        var mockUserManager = Fake.CreateUserManager<ApplicationUser>();
        mockUserManager
            .Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        mockUserManager
            .Setup(m => m.ConfirmEmailAsync(user, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var controller = new AccountController(_mapper,
            mockUserManager.Object,
            It.IsAny<SignInManager<ApplicationUser>>(),
            It.IsAny<ClaimsManager<ApplicationUser>>(),
            It.IsAny<IPublishEndpoint>());

        var result = await controller.ConfirmEmail(user.Id.ToString(), "code");

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("ConfirmEmail", viewResult.ViewName);
    }
    
    [Fact]
    public async Task ConfirmEmail_Get_ShouldReturnViewErrorBecauseUserIdOrCodeWasNull_ViewResult()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "example@mail.ru" };
        
        var mockUserManager = Fake.CreateUserManager<ApplicationUser>();
        mockUserManager
            .Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        mockUserManager
            .Setup(m => m.ConfirmEmailAsync(user, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var controller = new AccountController(_mapper,
            mockUserManager.Object,
            It.IsAny<SignInManager<ApplicationUser>>(),
            It.IsAny<ClaimsManager<ApplicationUser>>(),
            It.IsAny<IPublishEndpoint>());

        var result = await controller.ConfirmEmail(null!, null!);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Error", viewResult.ViewName);
    }
    
    [Fact]
    public async Task ConfirmEmail_Get_ShouldReturnViewErrorBecauseUserNotFound_ViewResult()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "example@mail.ru" };
        
        var mockUserManager = Fake.CreateUserManager<ApplicationUser>();
        mockUserManager
            .Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync((ApplicationUser)null!);
        mockUserManager
            .Setup(m => m.ConfirmEmailAsync(user, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var controller = new AccountController(_mapper,
            mockUserManager.Object,
            It.IsAny<SignInManager<ApplicationUser>>(),
            It.IsAny<ClaimsManager<ApplicationUser>>(),
            It.IsAny<IPublishEndpoint>());

        var result = await controller.ConfirmEmail(user.Id.ToString(), "code");

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Error", viewResult.ViewName);
    }
    
    [Fact]
    public async Task ConfirmEmail_Get_ShouldReturnViewErrorBecauseConfirmEmailWasFailed_ViewResult()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "example@mail.ru" };
        
        var mockUserManager = Fake.CreateUserManager<ApplicationUser>();
        mockUserManager
            .Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        mockUserManager
            .Setup(m => m.ConfirmEmailAsync(user, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed());

        var controller = new AccountController(_mapper,
            mockUserManager.Object,
            It.IsAny<SignInManager<ApplicationUser>>(),
            It.IsAny<ClaimsManager<ApplicationUser>>(),
            It.IsAny<IPublishEndpoint>());

        var result = await controller.ConfirmEmail(user.Id.ToString(), "code");

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Error", viewResult.ViewName);
    }

    [Fact]
    public void ForgotPassword_Get_ShouldReturnViewWithReturnUrl_ViewResult()
    {
        var controller = new AccountController(_mapper,
            It.IsAny<UserManager<ApplicationUser>>(),
            It.IsAny<SignInManager<ApplicationUser>>(),
            It.IsAny<ClaimsManager<ApplicationUser>>(),
            It.IsAny<IPublishEndpoint>());

        var result = controller.ForgotPassword(It.IsAny<string>());

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewData.ContainsKey("ReturnUrl"));
    }

    [Fact]
    public async Task ForgotPassword_Post_ShouldReturnForgotPasswordConfirmationWhenUserNotNullAndEmailIsConfirmed_ViewResult()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "example@mail.ru" };
        var model = new ForgotPasswordViewModel { Email = "example@mail.ru" };

        var mockUserManager = Fake.CreateUserManager<ApplicationUser>();
        mockUserManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        mockUserManager
            .Setup(m => m.IsEmailConfirmedAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(true);
        mockUserManager
            .Setup(m => m.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(It.IsAny<string>());

        var mockUrlHelper = Fake.CreateUrlHelper();
        var mockPublishEndpoint = Fake.CreatePublishEndpoint<MailMessageRequestContract>();

        var controller = new AccountController(_mapper,
            mockUserManager.Object,
            It.IsAny<SignInManager<ApplicationUser>>(),
            It.IsAny<ClaimsManager<ApplicationUser>>(),
            mockPublishEndpoint.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            },
            Url = mockUrlHelper.Object
        };

        var result = await controller.ForgotPassword(model, It.IsAny<string>());

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("ForgotPasswordConfirmation", viewResult.ViewName);
    }
    
    [Fact]
    public async Task ForgotPassword_Post_ShouldReturnViewErrorWhenUserNullOrEmailIsNotConfirmed_ViewResult()
    {
        var model = new ForgotPasswordViewModel { Email = "example@mail.ru" };

        var mockUserManager = Fake.CreateUserManager<ApplicationUser>();
        mockUserManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser)null!);
        mockUserManager
            .Setup(m => m.IsEmailConfirmedAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(false);
        mockUserManager
            .Setup(m => m.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(It.IsAny<string>());

        var mockUrlHelper = Fake.CreateUrlHelper();
        var mockPublishEndpoint = Fake.CreatePublishEndpoint<MailMessageRequestContract>();

        var controller = new AccountController(_mapper,
            mockUserManager.Object,
            It.IsAny<SignInManager<ApplicationUser>>(),
            It.IsAny<ClaimsManager<ApplicationUser>>(),
            mockPublishEndpoint.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            },
            Url = mockUrlHelper.Object
        };

        var result = await controller.ForgotPassword(model, It.IsAny<string>());

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Error", viewResult.ViewName);
    }

    [Fact]
    public async Task ForgotPassword_Post_ShouldReturnViewWhenModelIsInvalid_ViewResult()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "example@mail.ru" };
        var model = new ForgotPasswordViewModel { Email = "example@mail.ru" };

        var mockUserManager = Fake.CreateUserManager<ApplicationUser>();
        mockUserManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        mockUserManager
            .Setup(m => m.IsEmailConfirmedAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(true);
        mockUserManager
            .Setup(m => m.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(It.IsAny<string>());

        var mockUrlHelper = Fake.CreateUrlHelper();
        var mockPublishEndpoint = Fake.CreatePublishEndpoint<MailMessageRequestContract>();

        var controller = new AccountController(_mapper,
            mockUserManager.Object,
            It.IsAny<SignInManager<ApplicationUser>>(),
            It.IsAny<ClaimsManager<ApplicationUser>>(),
            mockPublishEndpoint.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            },
            Url = mockUrlHelper.Object
        };
        
        controller.ModelState.AddModelError("ModelError", "Some error occured while checking the model");

        var result = await controller.ForgotPassword(model, It.IsAny<string>());

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewData.ContainsKey("ReturnUrl"));
    }
    
    [Fact]
    public void ResetPassword_Get_ShouldReturnViewWithUserIdAndCodeAndReturnUrl_ViewResult()
    {
        var controller = new AccountController(_mapper,
            It.IsAny<UserManager<ApplicationUser>>(),
            It.IsAny<SignInManager<ApplicationUser>>(),
            It.IsAny<ClaimsManager<ApplicationUser>>(),
            It.IsAny<IPublishEndpoint>());

        var result = controller.ResetPassword("userId", "code", It.IsAny<string>());

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewData.ContainsKey("UserId"));
        Assert.True(viewResult.ViewData.ContainsKey("Code"));
        Assert.True(viewResult.ViewData.ContainsKey("ReturnUrl"));
    }

    [Fact]
    public async Task ResetPassword_Post_ShouldReturnResetPasswordConfirmationWithReturnUrlWhenPasswordWasSuccessfullyReset_ViewResult()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "example@mail.ru" };
        var model = new ResetPasswordViewModel { Password = "Qwerty123%", ConfirmPassword = "Qwerty123%" };

        var mockUserManager = Fake.CreateUserManager<ApplicationUser>();
        mockUserManager
            .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        mockUserManager
            .Setup(m => m.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var controller = new AccountController(_mapper,
            mockUserManager.Object,
            It.IsAny<SignInManager<ApplicationUser>>(),
            It.IsAny<ClaimsManager<ApplicationUser>>(),
            It.IsAny<IPublishEndpoint>());

        var result = await controller.ResetPassword(model, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewData.ContainsKey("ReturnUrl"));
    }
    
    [Fact]
    public async Task ResetPassword_Post_ShouldReturnResetPasswordViewWhenPasswordWasNotSuccessfullyReset_ViewResult()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "example@mail.ru" };
        var model = new ResetPasswordViewModel { Password = "Qwerty123%", ConfirmPassword = "Qwerty123%" };

        var mockUserManager = Fake.CreateUserManager<ApplicationUser>();
        mockUserManager
            .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        mockUserManager
            .Setup(m => m.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed());

        var controller = new AccountController(_mapper,
            mockUserManager.Object,
            It.IsAny<SignInManager<ApplicationUser>>(),
            It.IsAny<ClaimsManager<ApplicationUser>>(),
            It.IsAny<IPublishEndpoint>());

        var result = await controller.ResetPassword(model, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewData.ContainsKey("ReturnUrl"));
    }
    
    [Fact]
    public async Task ResetPassword_Post_ShouldReturnViewErrorWhenModelIsInvalid_ViewResult()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "example@mail.ru" };
        var model = new ResetPasswordViewModel { Password = "Qwerty123%", ConfirmPassword = "Qwerty123%" };

        var mockUserManager = Fake.CreateUserManager<ApplicationUser>();
        mockUserManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(user);
        mockUserManager
            .Setup(m => m.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed());

        var controller = new AccountController(_mapper,
            mockUserManager.Object,
            It.IsAny<SignInManager<ApplicationUser>>(),
            It.IsAny<ClaimsManager<ApplicationUser>>(),
            It.IsAny<IPublishEndpoint>());
        
        controller.ModelState.AddModelError("ModelError", "Some error occured while checking the model");

        var result = await controller.ResetPassword(model, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewData.ContainsKey("ReturnUrl"));
    }
}