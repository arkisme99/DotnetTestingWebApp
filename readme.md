#Fitur-fitur

- Pattern MVC + Service
- Components Views
- Authentication (Login, Logout)
- Multi Lang Label (id, en)
- Send Email
- Implementasi UUID
- Dark Mode Admin Lte
- Implementasi DataTable Server Side
- Role Management
- User Management + Multi Role
- Implementasi Job Using Hangfire
- Implementasi Activity Log (Manual & Otomatis)
- Implementasi Import & Export Excel
- Implementasi notification global pakai signalR (abstraksi untuk real-time communication)

#Set database on development

```
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=localhost;port=3306;database=database_name;user=root;password="
```

#command migrations

- dotnet ef migrations add Initial
- dotnet ef database update
- dotnet ef database drop

##Migration Host & Tenant
