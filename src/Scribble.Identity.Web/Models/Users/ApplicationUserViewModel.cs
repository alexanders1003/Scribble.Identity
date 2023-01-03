namespace Scribble.Identity.Web.Models.Users;

public class ApplicationUserViewModel
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
}