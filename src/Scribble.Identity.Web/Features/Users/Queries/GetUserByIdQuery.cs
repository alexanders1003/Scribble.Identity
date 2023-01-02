using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Web.Models.Users;

namespace Scribble.Identity.Web.Features.Users.Queries;

public class GetUserByIdQuery : IRequest<ApplicationUserViewModel>
{
    public GetUserByIdQuery(Guid id) => 
        Id = id;
    
    public Guid Id { get; }
}

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ApplicationUserViewModel>
{
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserByIdQueryHandler(IMapper mapper, UserManager<ApplicationUser> userManager)
    {
        _mapper = mapper;
        _userManager = userManager;
    }

    public async Task<ApplicationUserViewModel> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager
            .FindByIdAsync(request.Id.ToString())
            .ConfigureAwait(false);

        return _mapper.Map<ApplicationUserViewModel>(user);
    }
}