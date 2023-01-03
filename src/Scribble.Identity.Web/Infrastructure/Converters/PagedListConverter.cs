using AutoMapper;
using Calabonga.UnitOfWork;

namespace Scribble.Identity.Web.Infrastructure.Converters;

// ReSharper disable once ClassNeverInstantiated.Global
public class PagedListConverter<TMapFrom, TMapTo> : ITypeConverter<IPagedList<TMapFrom>, IPagedList<TMapTo>>
{
    public IPagedList<TMapTo> Convert(IPagedList<TMapFrom>? source, IPagedList<TMapTo> destination, ResolutionContext context) =>
        source == null
            ? PagedList.Empty<TMapTo>()
            : PagedList.From(source, items => context.Mapper.Map<IEnumerable<TMapTo>>(items));
}