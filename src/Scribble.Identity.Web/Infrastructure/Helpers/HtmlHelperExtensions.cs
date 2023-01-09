using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Scribble.Identity.Web.Infrastructure.Helpers;

public static class HtmlHelperExtensions
{
    public static IHtmlContent Logo(this IHtmlHelper helper, string? labelText = null)
    {
        return new HtmlString($"""
                 <div class="logo">
                     <div class="sign">
                         <div class="square"></div>  
                     </div> 
                     <span class="name">Scribble { labelText} </span>
                 </div>
                 """ );
    }

    public static IHtmlContent Title(this IHtmlHelper helper, string h3Title, string h4Title)
    {
        return new HtmlString($"""
                 <div class="title">
                     <h3>{ h3Title} </h3>
                     <h4>{ h4Title} </h4>
                 </div>
                 """ );
    }
}