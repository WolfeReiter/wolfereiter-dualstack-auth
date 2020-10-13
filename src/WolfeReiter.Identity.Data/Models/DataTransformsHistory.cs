using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WolfeReiter.Identity.Data.Models
{
    public class DataTransformsHistory
    {
        public DataTransformsHistory()
        {
            Applied = DateTime.UtcNow;
        }

        [MaxLength(150)]
        [Required]
        [Key]
        public string? TransformId { get; set; }

        [Required]
        public DateTime Applied { get; set; }

    }
}
