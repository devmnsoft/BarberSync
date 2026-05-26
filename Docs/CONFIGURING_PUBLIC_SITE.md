# CONFIGURING_PUBLIC_SITE.md

Consulte README.md para fluxo completo.

## Comandos
- docker compose up -d --build
- dotnet run --project Backend/Presentation/BarberSync.Api
- dotnet run --project Web/BarberSync.AdminWeb
- dotnet run --project Web/BarberSync.PublicWeb
- dotnet run --project Web/BarberSync.KioskWeb
- psql -U postgres -d barber -f ScriptsSQL/barber_full_database_postgresql.sql
- psql -U postgres -d barber -f ScriptsSQL/validate_barber_database.sql
