using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WolfeReiter.Identity.Data.Models.SqlServer
{
    public class Cache
    {
        //standard way is to create the schema with the dotnet-sql-cache global tool:
        //dotnet sql-cache create "connectionstring" dbo Cache
        //this Entity creates the same schema
        public Cache()
        {
            Id            = "";
            Value         = new byte[0];
            ExpiresAtTime = DateTimeOffset.Now;
        }
        [Key]
        [Required]
        public string Id { get; set; }

        [Required]
        public byte[] Value { get; set; }

        [Required]
        public DateTimeOffset ExpiresAtTime { get; set; }

        public long? SlidingExpirationInSeconds { get; set; }

        public DateTimeOffset? AbsoluteExpiration { get; set; }
    }
}
