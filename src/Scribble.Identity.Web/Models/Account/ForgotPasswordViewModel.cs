using System.ComponentModel.DataAnnotations;

namespace Scribble.Identity.Web.Models.Account;

public class ForgotPasswordViewModel
{
    [Required] 
    [EmailAddress] 
    public string Email { get; set; } = null!;
}