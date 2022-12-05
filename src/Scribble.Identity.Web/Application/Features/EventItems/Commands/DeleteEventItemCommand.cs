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

public class DeleteEventItemCommand : IRequest<EventItemViewModel>
{
    public DeleteEventItemCommand(Guid id) => Id = id;
    public Guid Id { get; }
}

public class DeleteEventItemCommandHandler : IRequestHandler<DeleteEventItemCommand, EventItemViewModel>
{
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
    private readonly IMapper _mapper;

    public DeleteEventItemCommandHandler(IUnitOfWork<ApplicationDbContext> unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<EventItemViewModel> Handle(DeleteEventItemCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<EventItem>();

        var entity = await repository.FindAsync(request.Id)
            .ConfigureAwait(false);

        if (entity == null)
            throw new MicroserviceEntityNotFoundException(typeof(EventItem),$"Entity with id '{request.Id}' not found.");

        repository.Delete(entity);

        await _unitOfWork.SaveChangesAsync()
            .ConfigureAwait(false);

        if (_unitOfWork.LastSaveChangesResult.IsOk)
            return _mapper.Map<EventItemViewModel>(entity);

        throw new MicroserviceException("Something went wrong.");
    }
}