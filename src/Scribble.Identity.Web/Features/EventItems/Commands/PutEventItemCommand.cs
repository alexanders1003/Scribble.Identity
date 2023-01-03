using AutoMapper;
using Calabonga.Microservices.Core.Exceptions;
using Calabonga.UnitOfWork;
using MediatR;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Infrastructure.Contexts;
using Scribble.Identity.Infrastructure.Exceptions;
using Scribble.Identity.Models;
using Scribble.Identity.Web.Models.EventItems;

namespace Scribble.Identity.Web.Application.Features.EventItems.Commands;

public class PutEventItemCommand : IRequest<EventItemViewModel>
{
    public PutEventItemCommand(Guid id, EventItemUpdateViewModel model)
    {
        Id = id;
        Model = model;
    }
    
    public Guid Id { get; }
    public EventItemUpdateViewModel Model { get; }
}

public class PutEventItemCommandHandler : IRequestHandler<PutEventItemCommand, EventItemViewModel>
{
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
    private readonly IMapper _mapper;

    public PutEventItemCommandHandler(IUnitOfWork<ApplicationDbContext> unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<EventItemViewModel> Handle(PutEventItemCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<EventItem>();

        var entity = await repository.GetFirstOrDefaultAsync(predicate: i => i.Id == request.Id, disableTracking: false)
            .ConfigureAwait(false);

        if (entity == null)
            throw new MicroserviceEntityNotFoundException(typeof(EventItem), $"Entity with id '{request.Id}' not found.");

        _mapper.Map(request.Model, entity);
        repository.Update(entity);

        await _unitOfWork.SaveChangesAsync()
            .ConfigureAwait(false);

        if (_unitOfWork.LastSaveChangesResult.IsOk)
            return _mapper.Map<EventItem, EventItemViewModel>(entity);

        throw new MicroserviceException("Something went wrong.");
    }
}