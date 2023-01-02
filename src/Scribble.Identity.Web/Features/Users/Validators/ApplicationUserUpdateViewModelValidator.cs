using FluentValidation;
using Scribble.Identity.Web.Models.Users;

namespace Scribble.Identity.Web.Features.Users.Validators;

public class ApplicationUserUpdateViewModelValidator : AbstractValidator<ApplicationUserUpdateViewModel>
{
    public ApplicationUserUpdateViewModelValidator() => RuleSet("user-update-rule", () =>
    {
        RuleFor(x => x.Email)
            .NotNull().NotEmpty().EmailAddress();
    });
}