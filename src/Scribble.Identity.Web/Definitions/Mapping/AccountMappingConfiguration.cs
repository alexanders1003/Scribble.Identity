using System.Security.Claims;
using Calabonga.Microservices.Core;
using OpenIddict.Abstractions;
using Scribble.Identity.Infrastructure;
using Scribble.Identity.Web.Models.Account;

namespace Scribble.Identity.Web.Definitions.Mapping;

public class AccountMappingConfiguration : AutoMapper.Profile
{
    public AccountMappingConfiguration()
    {
        CreateMap<SignUpViewModel, ApplicationUser>()
            .ForMember(x => x.UserName, o => o.MapFrom(p => p.Email))
            .ForMember(x => x.Email, o => o.MapFrom(p => p.Email))
            .ForMember(x => x.EmailConfirmed, o => o.Ignore())
            .ForMember(x => x.PhoneNumberConfirmed, o => o.Ignore())
            .ForMember(x => x.NormalizedUserName, o => o.Ignore())
            .ForMember(x => x.NormalizedEmail, o => o.Ignore())
            .ForMember(x => x.PasswordHash, o => o.Ignore())
            .ForMember(x => x.SecurityStamp, o => o.Ignore())
            .ForMember(x => x.ConcurrencyStamp, o => o.Ignore())
            .ForMember(x => x.PhoneNumber, o => o.Ignore())
            .ForMember(x => x.TwoFactorEnabled, o => o.Ignore())
            .ForMember(x => x.LockoutEnd, o => o.Ignore())
            .ForMember(x => x.LockoutEnabled, o => o.Ignore())
            .ForMember(x => x.AccessFailedCount, o => o.Ignore());

        CreateMap<ClaimsIdentity, UserViewModel>()
            .ForMember(x => x.Id,
                o => o.MapFrom(claims => ClaimsHelper.GetValue<Guid>(claims, OpenIddictConstants.Claims.Subject)))
            .ForMember(x => x.UserName,
                o => o.MapFrom(claims => ClaimsHelper.GetValue<string>(claims, OpenIddictConstants.Claims.Username)))
            .ForMember(x => x.Email,
                o => o.MapFrom(claims => ClaimsHelper.GetValue<string>(claims, OpenIddictConstants.Claims.Email)))
            .ForMember(x => x.FirstName,
                o => o.MapFrom(claims => ClaimsHelper.GetValue<string>(claims, OpenIddictConstants.Claims.GivenName)))
            .ForMember(x => x.LastName,
                o => o.MapFrom(claims => ClaimsHelper.GetValue<string>(claims, OpenIddictConstants.Claims.FamilyName)));
    }
}