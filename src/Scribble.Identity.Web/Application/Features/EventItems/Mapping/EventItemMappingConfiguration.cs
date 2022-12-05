using AutoMapper;
using Calabonga.UnitOfWork;
using Scribble.Identity.Models;
using Scribble.Identity.Web.Models.EventItems;

namespace Scribble.Identity.Web.Application.Features.EventItems.Mapping;

public class EventItemMappingConfiguration : Profile
{
    public EventItemMappingConfiguration()
    {
        CreateMap<EventItemCreateViewModel, EventItem>()
            .ForMember(x => x.Id, o => o.Ignore());

        CreateMap<EventItem, EventItemViewModel>();

        CreateMap<EventItem, EventItemUpdateViewModel>();

        CreateMap<EventItemUpdateViewModel, EventItem>()
            .ForMember(x => x.CreatedAt, o => o.Ignore())
            .ForMember(x => x.ThreadId, o => o.Ignore())
            .ForMember(x => x.ExceptionMessage, o => o.Ignore());

        CreateMap<IPagedList<EventItem>, IPagedList<EventItemViewModel>>()
            .ConvertUsing<PagedListConverter<EventItem, EventItemViewModel>>();
    }
}