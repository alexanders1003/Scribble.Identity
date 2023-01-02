using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Web.Models.Users;

namespace Scribble.Identity.Web.Features.Users.Commands;

public class PutUserCommand : IRequest
{
    public PutUserCommand(ApplicationUserUpdateViewModel model) => Model = model;
    
    public ApplicationUserUpdateViewModel Model { get; }
}

public class PutUserCommandHandler : IRequestHandler<PutUserCommand>
{
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public PutUserCommandHandler(IMapper mapper, UserManager<ApplicationUser> userManager)
    {
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<Unit> Handle(PutUserCommand request, CancellationToken cancellationToken)
    {
        var user = _mapper.Map<ApplicationUser>(request.Model);

        await _userManager.UpdateAsync(user)
            .ConfigureAwait(false);

        return Unit.Value;
    }
}