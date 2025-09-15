```
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=localhost;port=3306;database=database_name;user=root;password="

dotnet ef migrations add InitialCreate
dotnet ef database update
```
