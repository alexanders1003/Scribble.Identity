using Calabonga.UnitOfWork;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scribble.Identity.Web.Definitions.Identity.Authorization;
using Scribble.Identity.Web.Definitions.OpenIddict;
using Scribble.Identity.Web.Features.Users.Commands;
using Scribble.Identity.Web.Features.Users.Queries;
using Scribble.Identity.Web.Models.Users;

namespace Scribble.Identity.Web.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(AuthenticationSchemes = AuthenticationData.AuthenticationSchemes)]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator) => 
        _mediator = mediator;

    [HttpGet("{id:guid}")]
    [Authorize(Permissions.Users.View)]
    [ProducesResponseType(typeof(ApplicationUserViewModel), StatusCodes.Status200OK)]
    public async Task<ApplicationUserViewModel> GetUserById(Guid id) =>
        await _mediator.Send(new GetUserByIdQuery(id), HttpContext.RequestAborted)
            .ConfigureAwait(false);

    [HttpGet("paged")]
    [Authorize(Permissions.Users.View)]
    [ProducesResponseType(typeof(IPagedList<ApplicationUserViewModel>), StatusCodes.Status200OK)]
    public async Task<IPagedList<ApplicationUserViewModel>> GetUserPaged(int pageIndex, int pageSize) =>
        await _mediator.Send(new GetUserPagedQuery(pageIndex, pageSize), HttpContext.RequestAborted)
            .ConfigureAwait(false);

    [HttpPost]
    [Authorize(Permissions.Users.Create)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<Guid> PostUser(ApplicationUserCreateViewModel model) =>
        await _mediator.Send(new PostUserCommand(model), HttpContext.RequestAborted)
            .ConfigureAwait(false);

    [HttpPut]
    [Authorize(Permissions.Users.Edit)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task PutUser(ApplicationUserUpdateViewModel model) =>
        await _mediator.Send(new PutUserCommand(model), HttpContext.RequestAborted)
            .ConfigureAwait(false);

    [HttpDelete("{id:guid}")]
    [Authorize(Permissions.Users.Delete)]
    [ProducesResponseType(typeof(ApplicationUserViewModel), StatusCodes.Status200OK)]
    public async Task<ApplicationUserViewModel> DeleteUser(Guid id) =>
        await _mediator.Send(new DeleteUserCommand(id), HttpContext.RequestAborted)
            .ConfigureAwait(false);

}