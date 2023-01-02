using FluentValidation;
using Scribble.Identity.Web.Models.EventItems;

namespace Scribble.Identity.Web.Features.EventItems.Validators;

public class EventItemUpdateViewModelValidator : AbstractValidator<EventItemUpdateViewModel>
{
    public EventItemUpdateViewModelValidator() => RuleSet("event-item-update-rule", () =>
    {
        RuleFor(x => x.Message)
            .NotEmpty().NotNull().MaximumLength(4000);
        RuleFor(x => x.Level)
            .NotEmpty().NotNull().MaximumLength(50);
        RuleFor(x => x.Logger)
            .NotNull().NotEmpty().MaximumLength(255);
    });
}