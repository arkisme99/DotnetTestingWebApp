using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;

namespace DotnetTestingWebApp.Models
{
    public class AppTenantInfo : TenantInfo
    {
        // kamu bisa tambah properti lain, misalnya:
        public string? OwnerEmail { get; set; }
        public string? OwnerPhone { get; set; }
        // Tambahkan ini supaya bisa pakai koneksi per tenant
        public string? ConnectionString { get; set; }
    }
}