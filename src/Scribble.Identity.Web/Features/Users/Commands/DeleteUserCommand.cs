using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Web.Models.Users;

namespace Scribble.Identity.Web.Features.Users.Commands;

public class DeleteUserCommand : IRequest<ApplicationUserViewModel>
{
    public DeleteUserCommand(Guid id) => Id = id;
    public Guid Id { get; }
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ApplicationUserViewModel>
{
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteUserCommandHandler(IMapper mapper, UserManager<ApplicationUser> userManager)
    {
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<ApplicationUserViewModel> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user =  await _userManager.FindByIdAsync(request.Id.ToString())
            .ConfigureAwait(false);

        await _userManager.DeleteAsync(user!)
            .ConfigureAwait(false);

        return _mapper.Map<ApplicationUserViewModel>(user);
    }
}