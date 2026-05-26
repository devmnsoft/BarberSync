# SETUP WINDOWS VISUAL STUDIO

1. Instalar Visual Studio 2022 com workload **ASP.NET and web development**.
2. Abrir `Backend/BarberSync.sln` no Visual Studio 2022.
3. Definir startup múltiplo: API (`Backend/Presentation/BarberSync.Api`), AdminWeb (`Web/BarberSync.AdminWeb`) e KioskWeb (`Web/BarberSync.KioskWeb`).
4. Criar banco `barber` no PostgreSQL.
5. Executar `psql -U postgres -d barber -f ScriptsSQL/barber_full_database_postgresql.sql`.
6. Validar com `psql -U postgres -d barber -f ScriptsSQL/validate_barber_database.sql`.
7. Rodar API, AdminWeb e KioskWeb.
8. Rodar Mobile:
   - `cd MobileApp`
   - `npm install`
   - `npx expo start`
