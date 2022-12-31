using System.Linq.Expressions;
using AutoMapper;
using Calabonga.UnitOfWork;
using MediatR;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Infrastructure.Contexts;
using Scribble.Identity.Models;
using Scribble.Identity.Web.Models.EventItems;

namespace Scribble.Identity.Web.Application.Features.EventItems.Queries;

public class GetEventItemPagedQuery : IRequest<IPagedList<EventItemViewModel>>
{
    public GetEventItemPagedQuery(int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
    }

    public int PageIndex { get; init; }
    public int PageSize { get; init; }
}

public class GetEventItemPagedQueryHandler : IRequestHandler<GetEventItemPagedQuery, IPagedList<EventItemViewModel>>
{
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
    private readonly IMapper _mapper;

    public GetEventItemPagedQueryHandler(IUnitOfWork<ApplicationDbContext> unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IPagedList<EventItemViewModel>> Handle(GetEventItemPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedList = await _unitOfWork.GetRepository<EventItem>()
            .GetPagedListAsync(
                pageIndex: request.PageIndex,
                pageSize: request.PageSize,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (pagedList.PageIndex > pagedList.TotalPages)
        {
            pagedList = await _unitOfWork.GetRepository<EventItem>()
                .GetPagedListAsync(
                    pageIndex: 0,
                    pageSize: request.PageSize,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        return _mapper.Map<IPagedList<EventItemViewModel>>(pagedList);
    }
}