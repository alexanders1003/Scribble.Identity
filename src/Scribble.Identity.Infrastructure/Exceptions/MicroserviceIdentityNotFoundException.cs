namespace Scribble.Identity.Infrastructure.Exceptions;

[Serializable]
public class MicroserviceIdentityNotFoundException : Exception
{
    public MicroserviceIdentityNotFoundException(Type identityType)
        : base(MicroserviceDefaultExceptionMessages.IdentityNotFound) => IdentityType = identityType;
    public Type IdentityType { get; set; }
}