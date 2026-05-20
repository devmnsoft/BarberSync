# Arquitetura BarberSync 2.0

## Backend (ASP.NET + C#)
- Clean Architecture + DDD + CQRS
- API REST versionada `/api/v1`
- JWT auth + BCrypt para senha
- Redis para cache
- RabbitMQ/Kafka para eventos de domínio

## Módulos funcionais
- Usuários e perfis
- Clientes
- Agenda
- Serviços e estoque
- Financeiro
- Reconhecimento de serviços com IA
- Dashboards e relatórios

## Boas práticas
- SOLID
- Testes unitários e integração
- Auditoria de login e ações críticas
- HTTPS obrigatório
