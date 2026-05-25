# Backend

Arquitetura em camadas:
- Domain: Entidades, Value Objects, regras
- Application: Casos de uso, contratos, DTOs
- Infrastructure: EF Core, Redis, mensageria, integrações
- Presentation: API REST + JWT + Swagger
- Tests: unitário e integração

## Banco PostgreSQL barber
Schema único `barber`.

```bash
psql -U postgres -d barber -f ScriptsSQL/barber_full_database_postgresql.sql
psql -U postgres -d barber -f ScriptsSQL/validate_barber_database.sql
```

