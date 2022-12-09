using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Scribble.Identity.Web.Infrastructure.Extensions;

public static class ModelStateDictionaryExtensions
{
    public static void AddIdentityErrors(this ModelStateDictionary modelStateDictionary, IEnumerable<IdentityError> errors)
    {
        foreach (var error in errors)
        {
            modelStateDictionary.AddModelError(error.Code, error.Description);
        }
    }

    public static void AddSignInErrors(this ModelStateDictionary modelStateDictionary, SignInResult signInResult)
    {
        if (signInResult.Succeeded) return;
        
        modelStateDictionary.AddModelError("SignInError", "Invalid email or password.");
    }
}