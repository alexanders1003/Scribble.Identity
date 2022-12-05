using AutoMapper;
using Calabonga.Microservices.Core.Exceptions;
using Calabonga.UnitOfWork;
using MediatR;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Infrastructure.Contexts;
using Scribble.Identity.Models;
using Scribble.Identity.Web.Models.EventItems;

namespace Scribble.Identity.Web.Application.Features.EventItems.Commands;

public class PostEventItemCommand : IRequest<EventItemViewModel>
{
    public PostEventItemCommand(EventItemCreateViewModel model) => Model = model;
    public EventItemCreateViewModel Model { get; }
}

public class PostEventItemCommandHandler : IRequestHandler<PostEventItemCommand, EventItemViewModel>
{
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PostEventItemCommandHandler> _logger;

    public PostEventItemCommandHandler(IUnitOfWork<ApplicationDbContext> unitOfWork, IMapper mapper, ILogger<PostEventItemCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<EventItemViewModel> Handle(PostEventItemCommand request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<EventItemCreateViewModel, EventItem>(request.Model);
        if (entity == null)
            throw new MicroserviceUnauthorizedException();

        await _unitOfWork.GetRepository<EventItem>()
            .InsertAsync(entity, cancellationToken)
            .ConfigureAwait(false);
        
        await _unitOfWork.SaveChangesAsync()
            .ConfigureAwait(false);

        if (_unitOfWork.LastSaveChangesResult.IsOk)
            return _mapper.Map<EventItem, EventItemViewModel>(entity);

        throw new MicroserviceException("Something went wrong.");
    }
}