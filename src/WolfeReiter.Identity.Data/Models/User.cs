using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WolfeReiter.Identity.Data.Models
{
    public class User
    {
        public User()
        {
            UserId              = Guid.NewGuid();
            Created             = DateTime.UtcNow;
            Active              = true;
            FailedLoginAttempts = 0;
            Locked              = false;
        }

        [Key]
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserNumber { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string GivenName { get; set; }

        [MaxLength(50)]
        public string Surname { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MaxLength(256 / 8)]
        public byte[] Hash { get; set; }

        [Required]
        [MaxLength(256 / 8)]
        public byte[] Salt { get; set; }
        [Required]
        public DateTime Created { get; set; }

        [Required]
        public bool Active { get; set; }

        [Required]
        public int FailedLoginAttempts {get; set; }

        public DateTime? LastLoginAttempt { get; set; }

        [Required]
        public bool Locked { get; set; }

        public IList<UserRole> UserRoles { get; set; }

    }
}