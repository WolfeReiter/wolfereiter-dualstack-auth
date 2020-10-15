using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WolfeReiter.Identity.Data.Models
{
    public class UserRole
    {
        public UserRole()
        {
            UserRoleId = Guid.NewGuid();
        }

        [Key]
        [Required]
        public Guid UserRoleId { get; set; }

        [Required]
        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public User? User { get; set; }

        [Required]
        [ForeignKey("Role")]
        public Guid RoleId { get; set; }
        public Role? Role { get; set; }
    }
}