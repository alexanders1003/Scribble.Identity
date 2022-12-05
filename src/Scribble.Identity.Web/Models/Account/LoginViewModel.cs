using System.ComponentModel.DataAnnotations;

namespace Scribble.Identity.Web.Models.Account;

public class LoginViewModel
{
    [Required] 
    [EmailAddress]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string? Password { get; set; }
    
    [Display(Name = "Remember Me")]
    public bool RememberMe { get; set; }
}