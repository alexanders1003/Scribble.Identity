using Microsoft.AspNetCore.Mvc;

namespace Scribble.Identity.Web.Infrastructure.Extensions;

public static class ControllerBaseExtensions
{
    public static ActionResult RedirectToLocal(this ControllerBase controllerBase, string returnUrl)
    {
        if (controllerBase.Url.IsLocalUrl(returnUrl))
        {
            return controllerBase.Redirect(returnUrl);
        }

        return controllerBase.RedirectToAction("Index", "Home");
    }
}