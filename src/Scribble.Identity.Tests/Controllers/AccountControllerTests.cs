using AutoMapper;
using MassTransit;
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

    [Fact]
    public void SignIn_Get_ShouldReturnViewWithReturnUrl_ViewResult()
    {
        var controller = new AccountController(_mapper,
            It.IsAny<UserManager<ApplicationUser>>(),
            It.IsAny<SignInManager<ApplicationUser>>(),
            It.IsAny<IPublishEndpoint>());

        var result = controller.SignIn(It.IsAny<string>());

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewData.ContainsKey("ReturnUrl"));
    }
    
    [Fact]
    public async Task SignIn_Post_ShouldRedirectToReturnUrlBecauseModelIsValid_RedirectResult()
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
            .Returns(Task.FromResult(SignInResult.Success));

        var controller = new AccountController(_mapper,
            mockUserManager.Object,
            mockSignInManager.Object,
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
            .Setup(m => m.FindByNameAsync(user.UserName!))
            .Returns(Task.FromResult(user)!);

        var mockSignInManager = Fake.CreateSignInManager<ApplicationUser>();
        mockSignInManager
            .Setup(m => m.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false))
            .Returns(Task.FromResult(SignInResult.Success));

        var controller = new AccountController(_mapper,
            mockUserManager.Object,
            mockSignInManager.Object,
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

        var controller = new AccountController(_mapper,
            mockUserManager.Object,
            mockSignInManager.Object,
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
}