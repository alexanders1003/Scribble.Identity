using Microsoft.AspNetCore.Authentication.Cookies;
using OpenIddict.Validation.AspNetCore;

namespace Scribble.Identity.Web.Definitions.OpenIddict;

public static class AuthenticationData
{
    public const string AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme + "," +
                                                OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
}