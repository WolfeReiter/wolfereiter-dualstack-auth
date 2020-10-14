using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WolfeReiter.Identity.DualStack.Models
{
    public class ResetViewModel
    {

        [Display(Name = "Password")]
        [Required(ErrorMessage = "Please provide your password.")]
        [MinLength(10, ErrorMessage = "Password must have at least 10 characters.")]
        [Compare("Confirm", ErrorMessage = "Passwords must match.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Confirm your password")]
        [Required(ErrorMessage = "Please provide your password.")]
        [Compare("Password", ErrorMessage = "Passwords must match.")]
        [DataType(DataType.Password)]
        public string? Confirm { get; set; }
        public string? Token { get; set; }
    }
}