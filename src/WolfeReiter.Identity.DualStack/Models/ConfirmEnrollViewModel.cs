using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WolfeReiter.Identity.DualStack.Models
{
    public class ConfirmEnrollViewModel
    {

        string? username;
        [Display(Name = "Username")]
        [Required(ErrorMessage = "Please provide your username.")]
        public string? Username
        {
            get => username;
            set => username = value?.ToLower();
        }

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
        
        /// <summary>
        /// DISPLAY ONLY: Treat as read-only because the validated email is encrypted into the Token.
        /// </summary>
        /// <value></value>
        [DataType(DataType.EmailAddress)]
        [ReadOnly(true)]
        public string? Email { get; set; }

        /// <summary>
        /// Token contains the encrypted email address and a timestamp.
        /// </summary>
        /// <value></value>
        public string? Token { get; set; }
    }
}