# BarberSync

Arquitetura alvo:
- Backend/API ASP.NET Core (`Backend/Presentation/BarberSync.Api`)
- Admin Web ASP.NET Core MVC (`Web/BarberSync.AdminWeb`)
- Public Web ASP.NET Core MVC (`Web/BarberSync.PublicWeb`)
- Kiosk Web ASP.NET Core MVC (`Web/BarberSync.KioskWeb`)
- Mobile React Native/Expo (`MobileApp`)
- PostgreSQL banco `barber` schema `barber` (`ScriptsSQL`)

## Docker
1. `cp .env.example .env` (PowerShell: `copy .env.example .env`)
2. `docker compose up -d --build`
3. URLs:
   - API: http://localhost:8080/swagger
   - Admin: http://localhost:8081
   - Public: http://localhost:8082/barbearia-elite-demo
   - Kiosk: http://localhost:8083/setup
4. Validar banco:
   - `docker exec -it barbersync-postgres psql -U postgres -d barber -c "select count(*) from barber.tenants;"`
5. Reset: `docker compose down -v && docker compose up -d --build`

## Windows sem Docker
1. `createdb -U postgres barber`
2. `psql -U postgres -d barber -f ScriptsSQL/barber_full_database_postgresql.sql`
3. `psql -U postgres -d barber -f ScriptsSQL/validate_barber_database.sql`
4. `dotnet run --project Backend/Presentation/BarberSync.Api`
5. `dotnet run --project Web/BarberSync.AdminWeb`
6. `dotnet run --project Web/BarberSync.PublicWeb`
7. `dotnet run --project Web/BarberSync.KioskWeb`
8. Mobile:
   - `cd MobileApp`
   - `npm install`
   - `npx expo start`

## Usuário demo
- `admin@barbersync.com` / `Admin@123`

## Testes e validações
- `dotnet build`
- `dotnet test`
- `docker compose config`
- `docker compose up -d --build`
