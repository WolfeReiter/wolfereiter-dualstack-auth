using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WolfeReiter.Identity.Data.Models
{
    public class Role
    {
        public Role()
        {
            RoleId = Guid.NewGuid();
        }
        
        [Key]
        [Required]
        public Guid RoleId { get; set; }

        [Required]
        [MaxLength(250)]
        public string Name { get; set; }

        public IList<UserRole> UserRoles { get; set; }
    }
}