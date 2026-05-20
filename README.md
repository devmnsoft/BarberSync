# BarberSync 2.0

Projeto base completo para desenvolvimento e deployment com:
- Backend .NET (Clean Architecture + DDD + CQRS)
- Admin Blazor
- Mobile React Native (Expo)
- Totem (Web + Node API)
- IA (ML.NET)
- Scripts SQL PostgreSQL
- CI/CD com GitHub Actions

## Estrutura
Ver pastas `Backend`, `Frontend/Admin`, `MobileApp`, `Totem`, `ScriptsSQL` e `ML`.

## Execução rápida
1. Backend API:
   - `cd Backend/Presentation/BarberSync.Api`
   - `dotnet run`
2. Admin:
   - `cd Frontend/Admin`
   - `dotnet run`
3. Mobile:
   - `cd MobileApp && npm install && npm start`
4. Totem:
   - API: `cd Totem/api && npm install && npm run dev`
   - App: `cd Totem/app && npm install && npm start`

## Roadmap sugerido
Consulte `docs/ROADMAP.md`.
