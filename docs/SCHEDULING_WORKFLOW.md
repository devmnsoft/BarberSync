# Agenda Inteligente Premium

## Escopo e segurança

A Agenda usa o escopo de `tenant_id` e `branch_id` derivado da identidade autenticada. UUIDs aparecem somente em `option.value`, atributos `data-*`, rotas originadas por uma seleção ou payloads internos. Nenhum fluxo pede que uma pessoa digite identificadores técnicos. A API administrativa permanece protegida por autenticação e permissões; o canal público usa apenas rotas explicitamente anônimas, escopo configurado da unidade e validação no servidor.

Falhas nunca produzem sucesso local ou dados demonstrativos. As superfícies preservam a mensagem da API e exibem `traceId`/`X-Trace-Id` para suporte. Provedores externos não configurados devem permanecer `ProviderNotConfigured`/`Skipped`; não se simula WhatsApp, SMS ou e-mail.

## Disponibilidade

Um slot só é elegível quando está no futuro e dentro da antecedência mínima e da janela máxima da política ativa. A duração do serviço e o `buffer_minutes` compõem o intervalo ocupado. A consulta valida serviço e profissional ativos, vínculo `professional_services`, escala em `professional_working_hours`, pausa da escala, `professional_time_off` aprovado, `professional_schedule_blocks`, agendamentos sobrepostos e capacidade do recurso. O mesmo profissional ou recurso não pode ser usado duas vezes no mesmo intervalo. Feriados ou bloqueios de filial, quando configurados, também retiram slots.

A decisão é refeita no `POST`, dentro da operação de persistência; a grade visual nunca é fonte de verdade. Reagendamento desconsidera apenas o próprio agendamento e respeita `reschedule_limit_minutes`. Cancelamento requer motivo e respeita `cancellation_limit_minutes`. Exceções administrativas dependem de permissão explícita e motivo auditável.

## Fluxo administrativo

`/Scheduling` oferece modos dia, semana e mês, filtros selecionáveis, KPIs, slots clicáveis, skeleton, empty state e erro rastreável. Novo agendamento é um wizard: cliente, serviço, profissional, data, slot retornado pela API e resumo. O registro cria histórico e confirmação pendente conforme a política. Reagendar abre o seletor de slots; cancelar coleta um motivo; confirmar, check-in e no-show são transições explícitas. O botão de comanda usa o agendamento já selecionado e não expõe um campo de ID.

`/Scheduling/Waitlist` filtra por data, período e situação. Uma entrada seleciona cliente, serviço e profissional opcional. Oferecer horário registra a oferta e o evento `WaitlistSlotOffered`; converter reserva novamente o slot antes de criar o agendamento; cancelar exige motivo. Itens vencidos passam a `Expired` durante o processamento operacional.

`/Scheduling/Policies` configura antecedência, janela futura, limites, confirmação, depósito, lista de espera, online booking e intervalo. `/Scheduling/Resources` mantém cadeira, sala, lavatório, equipamento ou outro recurso com capacidade positiva.

## Autoatendimento público

`/agendar` carrega somente serviços e profissionais ativos e publicados, permite “qualquer disponível”, escolhe data e horário e só então solicita nome, telefone e e-mail opcional. O servidor verifica política online, identidade da unidade, disponibilidade e conflito antes de confirmar. API offline ou online booking desabilitado gera estado indisponível claro, sem agenda fabricada.

## Totem

O totem mantém `Kiosk:DeviceCode` obrigatório, sem query string ou código local padrão. Cards touch permitem selecionar serviço, consultar próximo slot, entrar na lista de espera e localizar um agendamento existente para check-in. A criação de comanda só ocorre quando a transição operacional permite e usa a sessão autenticada do dispositivo.

## Mobile

Os contratos são `GET /api/mobile/appointments`, `GET /api/mobile/appointments/availability`, `POST /api/mobile/appointments`, `POST /api/mobile/appointments/{id}/cancel` e `POST /api/mobile/appointments/{id}/reschedule`. A lista mostra próximos horários e confirmação; ações partem do card selecionado. A lista de espera, quando habilitada na política, reutiliza opções reais da unidade.

## Operação, equipe, comunicação, BI e PDV

A Operação do Dia lê a mesma agenda e destaca agendados, aguardando confirmação, confirmados, check-in, atendimento iniciado, cancelados, no-show, próximos da lista de espera e encaixes. Começar/finalizar atendimento mantém as regras da comanda/PDV. Escalas e afastamentos da Equipe retiram disponibilidade imediatamente.

Criação, reagendamento, cancelamento e confirmação persistem notificação InApp, evento de comunicação e histórico. Templates previstos: `AppointmentCreated`, `AppointmentReminder`, `AppointmentRescheduled`, `AppointmentCancelled`, `AppointmentConfirmed` e `WaitlistSlotOffered`. O outbox externo só é criado para canal realmente configurado.

O BI agrega criados, confirmados, cancelamentos, no-show rate, ocupação por profissional e recurso, tempo até atendimento, lista de espera e encaixes. Métrica sem fonte íntegra declara `sourceStatus` em vez de retornar zero como dado real.

## Validação

Cliente, serviço, data e horário são obrigatórios; o profissional é obrigatório quando a política assim define. Datas passadas e slots indisponíveis são rejeitados no navegador para feedback e novamente na API por segurança. Motivos são obrigatórios para cancelamento, no-show, reagendamento e encaixe excepcional. O gate `scripts/validate-ui-contracts.sh` bloqueia âncoras vazias, handlers vazios, `@page` em Views MVC, inputs ou placeholders de ID e texto operacional inacabado nas novas superfícies.

## Integração — Portal do Cliente (Sprint 51)

O fluxo client-scoped, seus limites de privacidade, eventos e comportamento sem provider estão documentados em [CLIENT_PORTAL_WORKFLOW.md](CLIENT_PORTAL_WORKFLOW.md). A integração não aceita identificadores técnicos digitados e não transforma intenção de pagamento em liquidação.

## Integração Marketing Studio

O contrato de integração, atribuição e segurança está documentado em [MARKETING_STUDIO_WORKFLOW.md](MARKETING_STUDIO_WORKFLOW.md). Esta integração usa apenas dados persistidos, preserva o escopo tenant/unidade e não simula provider, pagamento ou conversão.

## Integração com Marketplace & Parceiros

A atribuição comercial usa referências rastreáveis e escopo tenant/unidade. Eventos pendentes ou cancelados não confirmam comissão/payout; detalhes e contratos estão em `docs/PARTNERS_MARKETPLACE_WORKFLOW.md`.

## Integração Sprint 57 — Catálogo & Precificação

A operação consome a fonte central de preço, custo, margem, duração, visibilidade e breakdown descrita em [CATALOG_PRICING_WORKFLOW.md](CATALOG_PRICING_WORKFLOW.md). Benefícios e comissões permanecem pendentes até o evento comercial real; escopo de tenant/unidade e trilha de auditoria são obrigatórios.

## Sprint 58 · Atendimento 360

O contrato integrado, estados, transações e responsabilidades deste módulo estão documentados em [SERVICE_EXECUTION_CHECKOUT_WORKFLOW.md](SERVICE_EXECUTION_CHECKOUT_WORKFLOW.md). Eventos reais são correlacionados por `service_order_id`; preview não altera ledger e estados pendentes não são tratados como receita, consumo ou comissão paga.
