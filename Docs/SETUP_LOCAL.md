# Setup Local — BarberSync

## 1) Banco de dados
```bash
docker compose up -d postgres
psql -h localhost -U postgres -d barber -f ScriptsSQL/barber_full_database_postgresql.sql
psql -h localhost -U postgres -d barber -f ScriptsSQL/validate_barber_database.sql
```

## 2) Backend API
```bash
dotnet restore
dotnet build
dotnet test
```

## 3) Admin Web
```bash
# ajuste para o diretório real do front admin
npm install
npm run build
```

## 4) Mobile
```bash
cd MobileApp
npm install
npm test
```

## 5) Totem
```bash
cd Totem
npm install
npm run build
npm test
```

## Usuários demo
- `admin@barbersync.com` / `Admin@123`
- `gerente@barbeariaelite.com` / `Admin@123`
- `recepcao@barbeariaelite.com` / `Admin@123`
- `financeiro@barbeariaelite.com` / `Admin@123`
- `profissional@barbeariaelite.com` / `Admin@123`
- `cliente@demo.com` / `Admin@123`
- `totem@barbeariaelite.com` / `Admin@123`
