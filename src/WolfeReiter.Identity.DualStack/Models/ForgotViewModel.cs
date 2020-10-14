using System;
using System.ComponentModel.DataAnnotations;

namespace WolfeReiter.Identity.DualStack.Models
{
    public class ForgotViewModel
    {
        private string? username;
        [Display(Name = "Username")]
        [Required(ErrorMessage = "Please provide your username.")]
        public string? Username
        {
            get => username;
            set => username = value?.ToLower();
        }
    }
}