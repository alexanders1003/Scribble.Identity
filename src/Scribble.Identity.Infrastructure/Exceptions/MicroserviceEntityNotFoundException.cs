namespace Scribble.Identity.Infrastructure.Exceptions;

[Serializable]
public class MicroserviceEntityNotFoundException : Exception
{
    public MicroserviceEntityNotFoundException(Type entityType) 
        : base(MicroserviceDefaultExceptionMessages.EntityNotFound) => EntityType = entityType;

    public MicroserviceEntityNotFoundException(Type entityType, string message)
        : base(message) => EntityType = entityType;
    public Type EntityType { get; set; }
}