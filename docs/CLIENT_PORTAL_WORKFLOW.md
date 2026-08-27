# Portal do Cliente

## Objetivo

O Portal do Cliente é a área **client-scoped** do PublicWeb. Ele reúne agenda, histórico compartilhado, consentimentos, anamneses, orçamentos, planos, pagamentos pendentes, benefícios, avaliações, preferências e suporte sem apresentar identificadores internos.

## Acesso seguro

1. O cliente informa o código público da unidade e seu e-mail ou celular cadastrado.
2. `POST /api/client-portal/auth/request-code` sempre responde de forma neutra para impedir enumeração. O código aleatório de seis dígitos é persistido somente como SHA-256, expira em dez minutos e aceita no máximo cinco tentativas.
3. Se não houver provedor, a API retorna `ProviderNotConfigured`/`ManualDeliveryRequired`; não afirma que enviou. Em Development, a visualização do código exige `ClientPortal:ExposeDevelopmentCode=true` explicitamente.
4. A verificação bem-sucedida consome o código e cria token aleatório cuja forma persistida também é SHA-256. A sessão dura oito horas, tem escopo de tenant, unidade e cliente e é revogada no logout.
5. O PublicWeb guarda o token em cookie `HttpOnly`, `SameSite=Strict` e `Secure` fora de Development. O navegador usa somente o proxy same-origin.

## Privacidade e ações

Todas as consultas filtram simultaneamente `tenant_id`, `branch_id` e `client_id` derivados da sessão. Histórico visual inclui apenas `SharedWithClient`; notas técnicas não são projetadas. Cancelamento e reagendamento respeitam prazo da política e conflitos de agenda. Aprovação exige orçamento apresentado e não expirado. Intenção de pagamento apenas registra metadado/evento e jamais altera o status para pago.

Consentimento, orçamento, agenda, avaliação, suporte, preferência e intenção financeira gravam eventos em `client_portal_events`. Uma avaliação é aceita apenas para atendimento concluído e é única por atendimento; ocultação administrativa deve guardar motivo. Mensagens `Internal` de suporte nunca são retornadas ao cliente.

## Erros e operação

Falhas usam `ProblemDetails`, texto humanizado e `traceId`. Não há dados demonstrativos nem sucesso simulado. Canais externos devem consumir códigos `Pending`; criar o registro não equivale a entrega. Para incidentes, use o traceId nos logs sem solicitar UUID ao cliente.

## Integrações

* **Cliente 360:** consentimentos, anamneses, orçamentos, planos e eventos usam as tabelas canônicas.
* **Agenda:** agendamentos e políticas canônicas controlam listagem, cancelamento e reagendamento.
* **Comunicação:** eventos `ClientPortal*` permitem automações respeitando opt-in e provider.
* **Financeiro:** `client_payment_requests` referencia a origem e um pagamento real opcional; intenção não liquida saldo.
* **BI:** métricas devem agregar `client_portal_events` e avaliações. Em indisponibilidade, publicar `sourceStatus: unavailable`.
* **Mobile/Kiosk:** Mobile compartilha contratos REST; Kiosk continua condicionado a `Kiosk:DeviceCode` e pode adotar as ações client-scoped sem query string de dispositivo.
