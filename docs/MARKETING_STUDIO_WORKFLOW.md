# Marketing Studio — fluxo funcional

## Escopo e segurança

O Marketing Studio planeja crescimento comercial com dados persistidos e isolados por `tenant_id` e `branch_id`. O módulo não envia mensagens: campanhas externas somente entram em execução quando o provider correspondente está ativo no módulo Comunicação. `ProviderNotConfigured` é uma falha explícita e nunca é convertido em sucesso.

As telas administrativas exigem autenticação e permissões `Marketing.*`. Clientes, segmentos, templates, campanhas, cupons, landing pages e links são escolhidos em seletores; UUIDs existem apenas no payload após uma seleção real. Opt-out, preferências de notificação e suppression list permanecem sob governança de Comunicação.

## Fluxos

1. **Segmento:** o operador compõe critérios no builder. O backend valida nome, tipo e ao menos um critério, persiste `criteria_json` e recalcula somente segmentos dinâmicos no escopo autenticado.
2. **Campanha:** o wizard conduz objetivo, segmento, canal, template, oferta, landing, período e revisão. A criação sempre resulta em `Draft`. Edição de concluída é bloqueada; troca de segmento requer campanha pausada. Cancelamento exige motivo.
3. **Execução:** Marketing planeja; Comunicação valida provider, consentimento e suppression list e executa. Não existe fallback ou envio demonstrativo.
4. **Jornada:** gatilho, condições, ações e `deduplication_hours` evitam repetição. Ações podem criar comunicação, cupom, follow-up ou tarefa, mas pagamentos e benefícios dependem de confirmação real.
5. **Landing e link:** `/p/{slug}` resolve somente landing ativa e vigente. `/go/{publicSlug}` valida vigência, grava clique e redireciona. Tokens opacos são armazenados exclusivamente por SHA-256; o token cru não é persistido.
6. **Atribuição:** eventos `View`, `Click`, `BookingStarted`, `BookingCompleted`, `OrderCreated`, `CouponRedeemed`, `PortalLogin` e `ReviewSubmitted` alimentam snapshots e CSV. Quando não há fonte, a UI exibe estado vazio, nunca dados fabricados.
7. **Experimento:** exatamente duas variações, templates válidos e alocação total de 100%. Campanhas concluídas não aceitam novo experimento.

## Integrações

- **Comunicação:** templates, providers, outbox, preferências e suppression list.
- **Qualidade & Retenção:** detratores, promotores, recuperação e follow-ups como critérios/gatilhos.
- **Clube & Vendas:** assinatura, renovação, vouchers, gift cards e pedidos `PendingPayment`; benefício somente após pagamento real.
- **Agenda:** origem de `BookingStarted` e `BookingCompleted` e CTA de agendamento.
- **Cliente 360 / Portal:** histórico de campanha, visita, clique, cupom e oferta personalizada.
- **Analytics:** views, CTR, conversão, agendamentos, pedidos e receita atribuída; fonte ausente é indisponível.
- **Mobile:** `/api/mobile/marketing/offers`, `/campaigns` e `/track`.
- **PublicWeb / Totem:** somente slugs públicos e links rastreáveis; o Totem mantém `Kiosk:DeviceCode` obrigatório.

## Operação e LGPD

Dados de IP e user-agent servem apenas à atribuição e antifraude e devem seguir retenção e anonimização definidas pelo controlador. URLs públicas não expõem IDs. A autenticação administrativa não pode ser desativada. Relatórios CSV obedecem `Marketing.Reports.Export`.
