using FluentValidation;
using Scribble.Identity.Web.Models.Users;

namespace Scribble.Identity.Web.Features.Users.Validators;

public class ApplicationUserCreateViewModelValidator : AbstractValidator<ApplicationUserCreateViewModel>
{
    public ApplicationUserCreateViewModelValidator() => RuleSet("user-create-rule", () =>
    {
        RuleFor(x => x.Email)
            .NotNull().NotEmpty().EmailAddress();
        RuleFor(x => x.Password)
            .NotNull().NotEmpty();
    });
}