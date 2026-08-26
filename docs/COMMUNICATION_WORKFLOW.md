# Comunicação omnichannel

## Escopo e segurança
O módulo `/Communication` usa exclusivamente o gateway autenticado do Admin. A API exige JWT e deriva `tenant_id`, `branch_id` e usuário das claims assinadas. IDs são valores internos de selects, links e linhas; nenhum formulário solicita UUID digitável. As permissões são `Communication.Read`, `Communication.Manage`, `Communication.Send`, `Communication.Export`, `Notification.Read` e `Notification.Manage`.

## Canais, templates e providers
Os canais aceitos são InApp, Push, Email, Sms, WhatsApp e Webhook. Templates aceitam somente a lista fechada de tokens validada pela API. InApp representa persistência local real em `notification_inbox`. Integrações externas sem provider configurado devem permanecer `Skipped` ou `Failed`, com `ProviderNotConfigured`; salvar na outbox nunca significa entrega.

## Campanhas e automações
Campanhas selecionam template, canal e audiência por controles de seleção. Segmentos são chaves conhecidas, não IDs informados pelo operador. Automações aceitam eventos operacionais conhecidos e offsets validados. A preparação deve excluir opt-out e suppression list antes da fila e preservar idempotência do evento.

## Outbox, tentativas e Inbox
`communication_outbox` registra conteúdo, agenda, estado e motivo. Cada chamada ao provider pertence a `communication_delivery_attempts`. Retry é permitido apenas para `Failed`/`Skipped`; cancelamento apenas para `Pending`. A Inbox oferece leitura individual e em lote. A identidade autenticada e o escopo impedem leitura entre tenants.

## Preferências, consentimento e LGPD
Preferências registram canal, evento, decisão, origem e data. A suppression list exige motivo e prevalece sobre opt-in. Destinos externos inválidos ou sem consentimento são ignorados com motivo auditável. Alterações administrativas devem ser registradas nos eventos/auditoria da aplicação.

## Admin, Mobile, integrações e Analytics
Dashboard, Templates, Campanhas, Automações, Outbox, Inbox, Preferências e Relatórios usam loading, vazio, erro com traceId e `form-validation.js`. Mobile consome `/api/notifications/inbox` e `/api/notifications/preferences`, sem armazenamento alternativo. Eventos de Agenda, PDV, CRM, Financeiro e Estoque devem registrar `communication_events` com chave idempotente; falha de comunicação não reverte a transação de origem. Analytics pode agregar status, canal, opt-outs, pendência e `ProviderNotConfigured` diretamente das tabelas de comunicação.

## Relatórios e limitações conhecidas
`GET /api/communication/reports/export?type=delivery&from=AAAA-MM-DD&to=AAAA-MM-DD` exporta CSV validado. Nesta entrega não há credenciais de provider externo nem alegação de envio externo; a habilitação exige implementação real do provider, configuração segura e confirmação positiva do fornecedor.

## Alertas InApp da IA Operacional

Câmera inativa, provider não configurado, fila acumulada e sugestão vencendo geram somente Inbox/InApp persistida para administradores/gerentes, com deduplicação e link para revisão. Nenhum provider externo é simulado.
