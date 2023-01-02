using AutoMapper;
using Calabonga.UnitOfWork;
using MediatR;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Infrastructure.Contexts;
using Scribble.Identity.Web.Models.Users;

namespace Scribble.Identity.Web.Features.Users.Queries;

public class GetUserPagedQuery : IRequest<IPagedList<ApplicationUserViewModel>>
{
    public GetUserPagedQuery(int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
    }
    
    public int PageIndex { get; }
    public int PageSize { get; }
}

public class GetUserPagedQueryHandler : IRequestHandler<GetUserPagedQuery, IPagedList<ApplicationUserViewModel>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;

    public GetUserPagedQueryHandler(IMapper mapper, IUnitOfWork<ApplicationDbContext> unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<IPagedList<ApplicationUserViewModel>> Handle(GetUserPagedQuery request, CancellationToken cancellationToken)
    {
        var pagedList = await _unitOfWork.GetRepository<ApplicationUser>()
            .GetPagedListAsync(
                pageIndex: request.PageIndex,
                pageSize: request.PageSize,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (pagedList.PageIndex > pagedList.TotalPages)
        {
            pagedList = await _unitOfWork.GetRepository<ApplicationUser>()
                .GetPagedListAsync(
                    pageIndex: 0,
                    pageSize: request.PageSize,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        return _mapper.Map<IPagedList<ApplicationUserViewModel>>(pagedList);
    }
}