using Scribble.Identity.Models.Base;

namespace Scribble.Identity.Infrastructure;

public class ApplicationUserProfile : AuditableEntity
{
    public ApplicationUser? ApplicationUser { get; set; }
    public List<ApplicationPermission>? Permissions { get; set; }
}