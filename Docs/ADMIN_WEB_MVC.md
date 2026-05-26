# ADMIN WEB MVC

Projeto `Web/BarberSync.AdminWeb` em ASP.NET Core MVC com Controllers e Views `.cshtml`.

- Layout: `Views/Shared/_Layout.cshtml`
- Partials: `_Sidebar.cshtml`, `_Topbar.cshtml`
- Dashboard inicial: `DashboardController` + `Views/Dashboard/Index.cshtml`
- Consumo de API via `HttpClient` registrado em `Program.cs` usando `ApiBaseUrl`.

## Execução
1. Ajuste `appsettings.Development.json`.
2. Execute `dotnet run` no projeto AdminWeb.
