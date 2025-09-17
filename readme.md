#Apa yang ada di dalam ?

- Pattern MVC + Service
- Components Views
- Authentication (Login, Role & Permission)
- Multi Lang Label (id, en)
- Send Email
- Implementasi UUID

```
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=localhost;port=3306;database=database_name;user=root;password="

dotnet ef migrations add InitialCreate
dotnet ef database update
```

#reset migration

- Hapus folder migrations
- dotnet ef database drop
- dotnet ef migrations add Initial
- dotnet ef database update

#reset migration tanpa drop database

```
dotnet ef migrations add Initial --ignore-changes
```
