using System;
using System.ComponentModel.DataAnnotations;

namespace WolfeReiter.Identity.DualStack.Models
{
    public class EnrollViewModel
    {
        private string? email;
        [Display(Name = "Email address")]
        [Required(ErrorMessage = "Please provide your email.")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [MaxLength(100)]
        public string? Email
        {
            get => email;
            set => email = value?.ToLower();
        }
    }
}