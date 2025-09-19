using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetTestingWebApp.Models.Interface
{
    public interface IAuditableEntity
    {
        //ini digunakan jika di model ada extends, jadi perlu tambah manual di Model kolom di bawah ini, tidak bisa extend dari class AuditableEntity
        DateTime? CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }

        //soft delete
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
    }
}