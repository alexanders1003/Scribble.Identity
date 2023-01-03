namespace Scribble.Identity.Infrastructure.Exceptions;

public static class MicroserviceDefaultExceptionMessages
{
    public const string IdentityValidation = "Some validation errors occured while checking the user data";
    public const string IdentityNotFound = "User not found in the system";
    public const string EntityValidation = "Some errors occurred while checking the entity";
    public const string EntityNotFound = "Entity not found in the system";
    public const string RoleNotFound = "Role not found in the system";
}