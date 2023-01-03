using AutoMapper;
using Calabonga.UnitOfWork;
using MockQueryable.Moq;
using Moq;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Infrastructure.Contexts;
using Scribble.Identity.Web.Features.Users.Mapping;
using Scribble.Identity.Web.Features.Users.Queries;
using Scribble.Identity.Web.Models.Users;

namespace Scribble.Identity.Tests.Features.Users.Queries;

public class GetUserPagedQueryTests
{
    private readonly IMapper _mapper;

    public GetUserPagedQueryTests()
    {
        var mapperConfig = new MapperConfiguration(configurator =>
        {
            configurator.AddProfile<ApplicationUserMappingConfiguration>();
        });
        
        _mapper = mapperConfig.CreateMapper();
    }
    
    [Theory]
    [InlineData(0, 3)]
    [InlineData(0, 6)]
    [InlineData(0, 9)]
    public async Task GetUserPaged_ShouldReturn_UserListPaged(int pageIndex, int pageSize)
    {
        var users = new List<ApplicationUser> {
            new() { Id = Guid.NewGuid(), Email = "example1@mail.ru" },
            new() { Id = Guid.NewGuid(), Email = "example2@mail.ru" },
            new() { Id = Guid.NewGuid(), Email = "example3@mail.ru" }
        }.AsQueryable().BuildMock(); 
        
        var mockDbSet = Fake.CreateDbSet(users);
        
        var mockDbContext = new Mock<ApplicationDbContext>();
        mockDbContext
            .Setup(m => m.Set<ApplicationUser>())
            .Returns(mockDbSet.Object);
        
        var unitOfWork = new UnitOfWork<ApplicationDbContext>(mockDbContext.Object);

        var query = new GetUserPagedQuery(pageIndex, pageSize);
        var handler = new GetUserPagedQueryHandler(_mapper, unitOfWork);

        var result = await handler.Handle(query, It.IsAny<CancellationToken>());

        Assert.NotNull(result);
        Assert.Equal(users.Count(), result.Items.Count);
        foreach (var item in result.Items)
        {
            Assert.IsAssignableFrom<ApplicationUserViewModel>(item);
        }
    }
}