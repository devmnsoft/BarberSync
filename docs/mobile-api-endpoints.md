# Mobile API endpoints (MVP)

Base: `/api/mobile`

## Autenticação
- `POST /auth/login`

### Exemplo request
```json
{ "email": "profissional@barbersync.com", "password": "***" }
```

### Exemplo response
```json
{ "success": true, "message": "Login realizado com sucesso.", "data": { "accessToken": "mobile-demo-token", "expiresAtUtc": "2026-05-25T12:00:00Z" } }
```

## Dashboard
- `GET /dashboard`

## Plantões
- `GET /plantoes-disponiveis?page=1&pageSize=10`

## Convites
- `GET /convites`

## Pagamentos
- `GET /meus-pagamentos`

## Notificações
- `GET /notificacoes`

## Padrão
Todos endpoints retornam envelope:
```json
{ "success": true, "message": "...", "data": {} }
```
