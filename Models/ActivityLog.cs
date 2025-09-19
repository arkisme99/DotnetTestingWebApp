using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetTestingWebApp.Models
{
    public class ActivityLog : AuditableEntity
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? EntityName { get; set; }  // Nama model
        public string? EntityId { get; set; }    // Id dari model
        public string? Action { get; set; }      // Create, Update, Delete
        public string? ChangedBy { get; set; }   // UserId atau Username
        public string? Changes { get; set; }     // JSON string berisi perubahan

    }
}