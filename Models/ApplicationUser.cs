using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using DotnetTestingWebApp.Models.Interface;
using Microsoft.AspNetCore.Identity;

namespace DotnetTestingWebApp.Models
{
    public class ApplicationUser : IdentityUser, IAuditableEntity
    {
        public string? FullName { get; set; }

        [Column("created_at", TypeName = "timestamp")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at", TypeName = "timestamp")]
        public DateTime? UpdatedAt { get; set; }

        //soft delete
        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;
        [Column("deleted_at", TypeName = "timestamp")]
        public DateTime? DeletedAt { get; set; }
    }
}