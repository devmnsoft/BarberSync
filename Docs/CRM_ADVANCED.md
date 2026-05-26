# CRM Advanced (Phase 1)

Esta fase inicial adiciona um módulo funcional de CRM de leads com:

- Cadastro de lead com validação de duplicidade por telefone/e-mail.
- Atribuição de responsável comercial.
- Alteração de status com histórico e exigência de motivo para perda.
- Conversão de lead em cliente (com bloqueio de reconversão).
- Reabertura de lead perdido com justificativa.
- Registro de interações e observações.
- Endpoints para listar leads quentes e duplicidades.

## Endpoints implementados

- `GET /api/crm/leads?tenantId={tenantId}`
- `GET /api/crm/leads/{id}`
- `POST /api/crm/leads`
- `POST /api/crm/leads/{id}/assign`
- `POST /api/crm/leads/{id}/status`
- `POST /api/crm/leads/{id}/interactions`
- `POST /api/crm/leads/{id}/notes`
- `POST /api/crm/leads/{id}/convert-to-client`
- `POST /api/crm/leads/{id}/mark-lost`
- `POST /api/crm/leads/{id}/reopen`
- `GET /api/crm/leads/duplicates?tenantId={tenantId}`
- `GET /api/crm/leads/hot?tenantId={tenantId}`
