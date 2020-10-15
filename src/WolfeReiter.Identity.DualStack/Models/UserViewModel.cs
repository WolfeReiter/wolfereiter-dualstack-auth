using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace WolfeReiter.Identity.DualStack.Models
{
    public class NewUserViewModel : UserViewModel
    {
        [Display(Name = "Password")]
        [Required(ErrorMessage = "Please provide a password.")]
        [MinLength(10, ErrorMessage = "Password must have at least 10 characters.")]
        [Compare("Confirm", ErrorMessage = "Passwords must match.")]
        [DataType(DataType.Password)]
        public override string? Password { get; set; }

        [Display(Name = "Confirm password")]
        [Required(ErrorMessage = "Please provide the password.")]
        [Compare("Password", ErrorMessage = "Passwords must match.")]
        [DataType(DataType.Password)]
        public override string? Confirm { get; set; }
    }

    public class EditUserViewModel : UserViewModel
    {
        [Display(Name = "Password")]
        //0 length (means password not updated) or 10+ characters
        [RegularExpression("(^$)|.{10,}", ErrorMessage = "Password must have at least 10 characters.")]
        [Compare("Confirm", ErrorMessage = "Passwords must match.")]
        [DataType(DataType.Password)]
        public override string? Password { get; set; }

        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Passwords must match.")]
        [DataType(DataType.Password)]
        public override string? Confirm { get; set; }

        [ReadOnly(true)]
        public Guid UserId { get; set; }

        [ReadOnly(true)]
        public int UserNumber { get; set; }

        [Display(Name = "Enabled")]
        public bool Active { get; set; }

        [Display(Name = "Locked out")]
        public bool Locked { get; set; }
    }

    public abstract class UserViewModel
    {
        public UserViewModel()
        {
            Roles = new List<CheckboxItem<Guid>>();
        }
        private string? username;
        [Display(Name = "Username")]
        [Required(ErrorMessage = "Please provide a unique username.")]
        [MaxLength(50)]
        public string? Username
        {
            get => username;
            set => username = value?.ToLower();
        }

        private string? email;
        [Display(Name = "Email address")]
        [Required(ErrorMessage = "Please provide user's email.")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [MaxLength(100)]
        public string? Email
        {
            get => email;
            set => email = value?.ToLower();
        }

        public abstract string? Password { get; set; }
        public abstract string? Confirm { get; set; }

        [Display(Name = "Given name for user (optional)")]
        [MaxLength(50)]
        public string? GivenName { get; set; }

        [Display(Name = "Surname for user (optional)")]
        [MaxLength(50)]
        public string? Surname { get; set; }

        public List<CheckboxItem<Guid>> Roles { get; set; }
    }
}