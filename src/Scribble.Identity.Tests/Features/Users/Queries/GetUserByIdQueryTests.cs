using AutoMapper;
using Moq;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Web.Features.Users.Mapping;
using Scribble.Identity.Web.Features.Users.Queries;
using Scribble.Identity.Web.Models.Users;

namespace Scribble.Identity.Tests.Features.Users.Queries;

public class GetUserByIdQueryTests
{
    private readonly IMapper _mapper;

    public GetUserByIdQueryTests()
    {
        var mapperConfig = new MapperConfiguration(configurator =>
        {
            configurator.AddProfile<ApplicationUserMappingConfiguration>();
        });
        
        _mapper = mapperConfig.CreateMapper();
    }
    
    [Fact]
    public async Task GetUserById_WhenUserExists_UserViewModel()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid() };

        var userManagerMock = Fake.CreateUserManager<ApplicationUser>();
        userManagerMock
            .Setup(m => m.FindByIdAsync(user.Id.ToString()))
            .Returns(Task.FromResult(user)!);
        
        var query = new GetUserByIdQuery(user.Id);
        var handler = new GetUserByIdQueryHandler(_mapper, userManagerMock.Object);

        var result = await handler
            .Handle(query, It.IsAny<CancellationToken>());
        
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.IsAssignableFrom<ApplicationUserViewModel>(result);
    }
}