using FluentValidation;
using Scribble.Identity.Web.Models.EventItems;

namespace Scribble.Identity.Web.Application.Features.EventItems.Validators;

public class EventItemCreateCommandValidator : AbstractValidator<EventItemCreateViewModel>
{
    public EventItemCreateCommandValidator() => RuleSet("default", () =>
    {
        RuleFor(x => x.CreatedAt).NotNull();
        RuleFor(x => x.Message).NotEmpty().NotNull().MaximumLength(4000);
        RuleFor(x => x.Level).NotEmpty().NotNull().MaximumLength(50);
        RuleFor(x => x.Logger).NotNull().NotEmpty().MaximumLength(255);

        RuleFor(x => x.ThreadId).MaximumLength(50);
        RuleFor(x => x.ExceptionMessage).MaximumLength(4000);
    });
}