using System;
using System.ComponentModel.DataAnnotations;

namespace WolfeReiter.Identity.DualStack.Models
{
    public class LoginViewModel
    {
        public string? RedirectUrl { get; set; }

        private string? username;
        [Display(Name = "Username")]
        [Required(ErrorMessage = "Please provide your username.")]
        public string? Username
        {
            get => username;
            set => username = value?.ToLower();
        }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "Please provide your password.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public string? Confirm { get; set; }
    }
}