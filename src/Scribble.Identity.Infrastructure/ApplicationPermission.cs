using Scribble.Identity.Models.Base;

namespace Scribble.Identity.Infrastructure;

public class ApplicationPermission : AuditableEntity
{
    public Guid ApplicationUserProfileId { get; set; }
    
    public ApplicationUserProfile? ApplicationUserProfile { get; set; }
    public string PolicyName { get; set; } = null!;
    public string Description { get; set; } = null!;
}