using Microsoft.AspNetCore.Identity;

namespace Scribble.Identity.Infrastructure;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    public Guid? ApplicationUserProfileId { get; set; }
    public ApplicationUserProfile? ApplicationUserProfile { get; set; }
}