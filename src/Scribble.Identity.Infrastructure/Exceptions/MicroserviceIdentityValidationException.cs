using Microsoft.AspNetCore.Identity;

namespace Scribble.Identity.Infrastructure.Exceptions;

[Serializable]
public class MicroserviceIdentityValidationException : Exception
{
    public MicroserviceIdentityValidationException(IEnumerable<IdentityError> errors)
        : base(MicroserviceDefaultExceptionMessages.IdentityValidation) => Errors = errors;
    public IEnumerable<IdentityError>? Errors { get; }
}