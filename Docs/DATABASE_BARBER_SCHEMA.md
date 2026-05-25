# BarberSync PostgreSQL Schema
Use banco `barber` com schema único `barber`.

```bash
psql -U postgres -d barber -f ScriptsSQL/barber_full_database_postgresql.sql
psql -U postgres -d barber -f ScriptsSQL/validate_barber_database.sql
```
