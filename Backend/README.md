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



## Evolução Comercial (Fase 1)
- CRM avançado de leads com conversão, perda/reabertura, histórico de status e interações.
- Endpoints REST em `/api/crm/leads` para operação comercial no Admin/Copilot.
- SQL inicial com `barber.crm_leads`, `barber.crm_lead_status_history` e view `barber.vw_lead_conversion_summary`.
