using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetTestingWebApp.Models
{
    public abstract class AuditableEntity
    {
        [Column("created_at", TypeName = "timestamp")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at", TypeName = "timestamp")]
        public DateTime? UpdatedAt { get; set; }
    }
}