# DATABASE_POSTGRESQL_FULL

## Visão geral
O arquivo consolidado do banco PostgreSQL do BarberSync está em:

- `ScriptsSQL/barbersync_full_database_postgresql.sql`

Ele reúne os scripts SQL existentes em `ScriptsSQL/` em ordem alfanumérica, com cabeçalhos por arquivo de origem, e já inclui as extensões:

- `pgcrypto`
- `citext`

## Como executar
```bash
psql -U postgres -d barber -f ScriptsSQL/barber_full_database_postgresql.sql
```

## Ordem de execução
1. Criação das extensões.
2. Execução sequencial dos blocos consolidados (scripts existentes).
3. Seed e views conforme scripts de origem.

## Validação pós-execução
Arquivo de validação:

- `ScriptsSQL/validate_barber_database.sql`

Execute:
```bash
psql -U postgres -d barber -f ScriptsSQL/validate_barber_database.sql
```

Valida:
- Schemas obrigatórios.
- Existência de tabelas principais.
- Existência de views de dashboard.
- Dados de seed (usuários, tenants, serviços, produtos).
- Índices essenciais.

## Usuários e dados demo
Os usuários e dados demo são carregados pelos scripts de seed já existentes no diretório `ScriptsSQL/` e agora fazem parte do consolidado.

## Reset do banco
```bash
dropdb barber
createdb barber
psql -U postgres -d barber -f ScriptsSQL/barber_full_database_postgresql.sql
```

## Conferência manual útil
```sql
SELECT schema_name FROM information_schema.schemata WHERE schema_name='barber';
SELECT COUNT(*) FROM barber.users;
SELECT COUNT(*) FROM barber.tenants;
SELECT COUNT(*) FROM barber.services;
SELECT COUNT(*) FROM barber.products;
SELECT COUNT(*) FROM barber.kiosk_devices;
```

## Appsettings (PostgreSQL)
Ajuste a connection string para PostgreSQL no arquivo `Backend/Presentation/appsettings.json`.
Exemplo:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=barber;Username=postgres;Password=postgres"
}
```
