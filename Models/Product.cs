using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetTestingWebApp.Models
{
    public class Product : AuditableEntity
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("name", TypeName = "varchar(255)")]
        [Required(ErrorMessage = "Nama wajib diisi")]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Column("price", TypeName = "decimal(15,6)")]
        [Required(ErrorMessage = "Harga wajib diisi")]
        [Range(typeof(decimal), "1", "9999999999999999", ErrorMessage = "Harga minimal 1")]
        public decimal? Price { get; set; }


    }
}