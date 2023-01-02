using AutoMapper;
using Calabonga.UnitOfWork;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Web.Infrastructure.Converters;
using Scribble.Identity.Web.Models.Users;

namespace Scribble.Identity.Web.Features.Users.Mapping;

public class ApplicationUserMappingConfiguration : Profile
{
    public ApplicationUserMappingConfiguration()
    {
        CreateMap<ApplicationUser, ApplicationUserViewModel>()
            .ForMember(x => x.Id, i => i.MapFrom(m => m.Id))
            .ForMember(x => x.UserName, i => i.MapFrom(m => m.UserName))
            .ForMember(x => x.Email, i => i.MapFrom(m => m.Email));

        CreateMap<ApplicationUserCreateViewModel, ApplicationUser>()
            .ForMember(x => x.Email, i => i.MapFrom(m => m.Email))
            .ForMember(x => x.EmailConfirmed, i => i.MapFrom(m => true));

        CreateMap<ApplicationUserUpdateViewModel, ApplicationUser>()
            .ForMember(x => x.Email, i => i.MapFrom(m => m.Email))
            .ForMember(x => x.EmailConfirmed, i => i.MapFrom(m => true));
        
        CreateMap<IPagedList<ApplicationUser>, IPagedList<ApplicationUserViewModel>>()
            .ConvertUsing<PagedListConverter<ApplicationUser, ApplicationUserViewModel>>();
    }
}