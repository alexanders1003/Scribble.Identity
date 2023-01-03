﻿using System.ComponentModel.DataAnnotations;

namespace Scribble.Identity.Web.Models.Account;

public class SignInViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = null!;
    
    [Display(Name = "Remember Me")]
    public bool RememberMe { get; set; }
}