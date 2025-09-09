using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetTestingWebApp.Models
{
    public class Product
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name", TypeName = "varchar(255)")]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Column("price", TypeName = "decimal(15,6)")]
        public decimal Price { get; set; }

        [Column("created_at", TypeName = "timestamp")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at", TypeName = "timestamp")]
        public DateTime? UpdatedAt { get; set; }
    }
}