using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Models;
using Scribble.Identity.Web.Models.Users;

namespace Scribble.Identity.Web.Features.Users.Commands;

public class PostUserCommand : IRequest<Guid>
{
    public PostUserCommand(ApplicationUserCreateViewModel model) => 
        Model = model;
    public ApplicationUserCreateViewModel Model { get; }
}

public class PostUserCommandHandler : IRequestHandler<PostUserCommand, Guid>
{
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;
    
    public PostUserCommandHandler(IMapper mapper, UserManager<ApplicationUser> userManager)
    {
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<Guid> Handle(PostUserCommand request, CancellationToken cancellationToken)
    {
        var user = _mapper.Map<ApplicationUser>(request.Model);

        await _userManager.CreateAsync(user, request.Model.Password)
            .ConfigureAwait(false);

        await _userManager.AddToRoleAsync(user, DefaultRoles.User.ToString())
            .ConfigureAwait(false);

        return user.Id;
    }
}