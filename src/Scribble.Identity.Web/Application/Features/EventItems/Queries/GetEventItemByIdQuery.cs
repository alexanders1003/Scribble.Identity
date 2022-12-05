using AutoMapper;
using Calabonga.UnitOfWork;
using MediatR;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Infrastructure.Contexts;
using Scribble.Identity.Infrastructure.Exceptions;
using Scribble.Identity.Models;
using Scribble.Identity.Web.Controllers;
using Scribble.Identity.Web.Models.EventItems;

namespace Scribble.Identity.Web.Application.Features.EventItems.Queries;

public class GetEventItemByIdQuery : IRequest<EventItemViewModel>
{
    public GetEventItemByIdQuery(Guid id) => Id = id;
    public Guid Id { get; init; }
}

public class GetEventItemByIdQueryHandler : IRequestHandler<GetEventItemByIdQuery, EventItemViewModel>
{
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
    private readonly IMapper _mapper;

    public GetEventItemByIdQueryHandler(IUnitOfWork<ApplicationDbContext> unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<EventItemViewModel> Handle(GetEventItemByIdQuery request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<EventItem>();

        var entity = await repository
            .GetFirstOrDefaultAsync(predicate: i => i.Id == request.Id)
            .ConfigureAwait(false);

        if (entity == null)
            throw new MicroserviceEntityNotFoundException(typeof(EventItem), $"Entity with id '{request.Id}' not found.");

        return _mapper.Map<EventItemViewModel>(entity);
    }
}