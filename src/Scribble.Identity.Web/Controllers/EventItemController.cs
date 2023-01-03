using Calabonga.UnitOfWork;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scribble.Identity.Web.Application.Features.EventItems.Commands;
using Scribble.Identity.Web.Definitions.OpenIddict;
using Scribble.Identity.Web.Features.EventItems.Commands;
using Scribble.Identity.Web.Features.EventItems.Queries;
using Scribble.Identity.Web.Infrastructure.Attributes;
using Scribble.Identity.Web.Models.EventItems;

namespace Scribble.Identity.Web.Controllers;

[ApiController]
[Route("api/event-items")]
[Authorize(AuthenticationSchemes = AuthenticationData.AuthenticationSchemes)]
public class EventItemController : ControllerBase
{
    private readonly IMediator _mediator;
    public EventItemController(IMediator mediator) => 
        _mediator = mediator;

    [HttpGet("{id:guid}")]
    [FeatureGroupName("EventItems")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<EventItemViewModel> GetLogById(Guid id) =>
        await _mediator.Send(new GetEventItemByIdQuery(id), HttpContext.RequestAborted)
            .ConfigureAwait(false);

    [HttpGet("paged")]
    [FeatureGroupName("EventItems")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IPagedList<EventItemViewModel>> GetLogPaged(int pageIndex, int pageSize) =>
        await _mediator.Send(new GetEventItemPagedQuery(pageIndex, pageSize), HttpContext.RequestAborted)
            .ConfigureAwait(false);

    [HttpPost]
    [FeatureGroupName("EventItems")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<EventItemViewModel> PostLog(EventItemCreateViewModel model)
        => await _mediator.Send(new PostEventItemCommand(model), HttpContext.RequestAborted)
            .ConfigureAwait(false);

    [HttpPut("{id:guid}")]
    [FeatureGroupName("EventItems")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<EventItemViewModel> PutLog(Guid id, EventItemUpdateViewModel model)
        => await _mediator.Send(new PutEventItemCommand(id, model), HttpContext.RequestAborted)
            .ConfigureAwait(false);

    [HttpDelete("{id:guid}")]
    [FeatureGroupName("EventItems")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<EventItemViewModel> DeleteLog(Guid id)
        => await _mediator.Send(new DeleteEventItemCommand(id), HttpContext.RequestAborted)
            .ConfigureAwait(false);
}