# DATABASE_POSTGRESQL_FULL

## Visão geral
O arquivo consolidado do banco PostgreSQL do BarberSync está em:

- `ScriptsSQL/barbersync_full_database_postgresql.sql`

Ele reúne os scripts SQL existentes em `ScriptsSQL/` em ordem alfanumérica, com cabeçalhos por arquivo de origem, e já inclui as extensões:

- `pgcrypto`
- `citext`

## Como executar
```bash
psql -U postgres -d barbersync -f ScriptsSQL/barbersync_full_database_postgresql.sql
```

## Ordem de execução
1. Criação das extensões.
2. Execução sequencial dos blocos consolidados (scripts existentes).
3. Seed e views conforme scripts de origem.

## Validação pós-execução
Arquivo de validação:

- `ScriptsSQL/validate_barbersync_database.sql`

Execute:
```bash
psql -U postgres -d barbersync -f ScriptsSQL/validate_barbersync_database.sql
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
dropdb barbersync
createdb barbersync
psql -U postgres -d barbersync -f ScriptsSQL/barbersync_full_database_postgresql.sql
```

## Conferência manual útil
```sql
SELECT schema_name FROM information_schema.schemata WHERE schema_name='identity';
SELECT COUNT(*) FROM identity.users;
SELECT COUNT(*) FROM tenant.tenants;
SELECT COUNT(*) FROM scheduling.services;
SELECT COUNT(*) FROM inventory.products;
SELECT COUNT(*) FROM dashboard.vw_admin_dashboard;
```

## Appsettings (PostgreSQL)
Ajuste a connection string para PostgreSQL no arquivo `Backend/Presentation/appsettings.json`.
Exemplo:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=barbersync;Username=postgres;Password=postgres"
}
```
