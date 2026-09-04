# Contratos de rotas da API — Sprint Funcional 34

Auditoria estática concluída em 24 de agosto de 2026 sobre **294 actions HTTP** em
`BarberSync.Api`. A tabela registra os contratos operacionais consumidos pelas
interfaces; rotas CRUD com `/{id}` mantêm o mesmo prefixo e política da linha do
módulo. A política fallback do host exige usuário autenticado, e `Authorize`
indica também a declaração explícita no controller.

| Módulo | Método | Rota | Controller/action | Autenticação | Permissão/role | Consumidor | Status |
|---|---|---|---|---|---|---|---|
| Auth | POST | `/api/auth/login` | Auth/Login | Pública | — | Admin/Mobile | OK |
| Dashboard | GET | `/api/dashboard/summary` | Dashboard/Summary | Sim (`Authorize`) | sessão | Admin | Proteção explícita corrigida |
| Appointments | GET/POST | `/api/appointments` | Appointments/List/Create | Sim (`Authorize`) | `Appointment.Read/Create` | Admin | OK |
| Appointments | POST | `/api/appointments/{id}/check-in` | Appointments/CheckIn | Sim (`Authorize`) | `Attendance.CheckIn` | Operação do Dia | transição idempotente por estado; duplicação rejeitada |
| Appointments | POST | `/api/appointments/{id}/start`, `/{id}/finish` | Appointments/Start/Finish | Sim (`Authorize`) | `Attendance.Start/Finish` | Operação do Dia | início abre/vincula comanda pelo consumidor antes da transição |
| Appointments | POST | `/api/appointments/{id}/cancel` | Appointments/Cancel | Sim | `Appointment.Cancel` | Admin | motivo no DTO |
| Appointments | POST | `/api/appointments/{id}/no-show` | Appointments/NoShow | Sim | `Appointment.Update` | Operação do Dia | motivo obrigatório |
| Appointments | GET | `/api/appointments/smart-slots` | Appointments/SmartSlots | Sim | `Appointment.Read` | Admin | OK |
| Clients | CRUD | `/api/clients[/{id}]` | Clients | Sim (fallback) | sessão | Admin | OK |
| Professionals | CRUD | `/api/professionals[/{id}]` | Professionals | Sim (fallback) | sessão | Admin | OK |
| Services | CRUD | `/api/services[/{id}]` | Services | Sim (fallback) | sessão | Admin | OK |
| Products | CRUD | `/api/products[/{id}]` | Products | Sim (fallback) | sessão | Admin | OK |
| Stock | GET/POST | `/api/stock`, `/api/stock/movements`, `/api/stock/{entry\|exit\|adjustment}` | Stock | Sim (`Authorize`) | `Stock.*` | Admin | produto e movimentos reais |
| Purchases | CRUD/POST | `/api/purchases[/{id}]`, `/{id}/receive` | Purchases | Sim (`Authorize`) | Manager+ na escrita | Admin | OK |
| ServiceOrders | POST | `/api/service-orders/open` | ServiceOrders/Open | Sim (`Authorize`) | `ServiceOrder.Create` | Admin | OK |
| ServiceOrders | GET | `/api/service-orders[/{id}]` | ServiceOrders/List/Get | Sim (`Authorize`) | `ServiceOrder.Read` | Operação/PDV | somente tenant e unidade dos claims |
| ServiceOrders | POST/DELETE | `/api/service-orders/{id}/items/{services\|products\|{itemId}}` | ServiceOrders items | Sim | `ServiceOrder.Update` | Admin | motivo exigido na remoção |
| Payments | POST | `/api/service-orders/{id}/payments` | ServiceOrders/Pay | Sim | `Payment.Create` | Admin PDV | contrato canônico |
| Payments | CRUD | `/api/payments[/{id}]` | Payments | Sim (`Authorize`) | sessão | Admin | Proteção explícita corrigida |
| CashRegisters | GET/POST | `/api/cash-registers/current`, `/open`, `/{id}/{supply\|withdrawal\|expense\|close}` | CashRegisters | Sim (`Authorize`) | `Cash.*` | Admin | OK |
| CashRegisters | GET | `/api/cash-registers/history` | CashRegisters/History | Sim (`Authorize`) | `Cash.View` | Operação/Caixa | turnos da unidade; movimentos expõem `paymentId` |
| Financial | GET | `/api/finance` | Finance/GetAll | Sim (`Authorize`) | sessão | Admin | OK |
| Commissions | GET | `/api/commissions` | Commissions/GetAll | Sim (`Authorize`) | Manager+ | Admin | visão financeira geral; profissional usa `/api/mobile/professional/commissions` com ownership |
| Reports | GET | `/api/executive/{owner\|reception\|export.csv}` | Executive | Sim (`Authorize`) | roles por action | Admin | período compartilhado |
| Notifications | GET/POST | `/api/notifications`, `/{id}/read`, `/read-all` | Notifications | Sim (`Authorize`) | sessão | Admin | Proteção explícita corrigida |
| Audit | GET | `/api/audit`, `/events`, `/{id}` | Audit | Sim (`Authorize`) | sessão | Admin | Proteção explícita corrigida |
| ServiceRecognition | GET/POST | `/api/service-recognition/*` | ServiceRecognition | Sim (`Authorize`) | Owner a Reception | Admin | confirmação humana |
| AiSettings | GET/POST | `/api/system/ai-settings`, `/test` | AiSettings | Sim (`Authorize`) | Owner/Admin | Admin | OK |
| Copilot | GET/POST | `/api/copilot/{conversations\|messages\|ask\|suggestions\|actions\|feedback}` | Copilot | Sim (`Authorize`) | tenant da sessão | Admin | isolamento preservado |
| Totem/Public | GET | `/api/kiosk/branches`, `/services`, `/professionals`, `/availability` | Kiosk | Pública controlada | device + tenant/branch configurados | Totem | métodos e queries conferidos |
| Totem/Public | POST | `/api/kiosk/client/find-by-phone`, `/client/quick-register`, `/check-in`, `/pre-orders` | Kiosk | Pública controlada | device + tenant/branch configurados | Totem | payloads conferidos |
| Mobile Client | GET | `/api/mobile/summary` | MobileSelfService/Summary | Sim (`Authorize`) | ownership | Mobile | **rota ausente corrigida** |
| Mobile Client | GET/POST | `/api/mobile/appointments/*`, `/client/*`, `/notifications/*` | MobileSelfService | Sim (`Authorize`) | ownership | Mobile | OK |
| Mobile Professional | GET/POST | `/api/mobile/professional/*` | MobileSelfService | Sim (`Authorize`) | agenda/comissão próprias | Mobile | OK |

## Correções e pendências verificáveis

- Removido o controller mobile paralelo que publicava login com token fabricado,
  plantões, convites, pagamentos e notificações em memória sob o mesmo prefixo.
- Implementado o resumo que `MobileApp/App.js` já consumia, projetando apenas
  agendamentos, benefícios, notificações e comissões pertencentes à identidade.
- Dashboard, pagamentos CRUD, notificações e auditoria agora expressam
  `Authorize` localmente, além da política fallback global.
- Admin, Totem e os contratos Mobile foram confrontados com seus métodos e
  querystrings. Não há referência a `PublicConfigController` ou
  `ConfigurationService` nos consumidores publicados.
- Pendente: build .NET, aplicação SQL e smoke autenticado/runtime no gate externo;
  a auditoria estática não promove o candidato a GO.


## Sprint de Produção 24

- O smoke HTTP passou a exigir `401`, sem faixa de status permissiva, também em `GET /api/mobile/summary`, e exige correlação do erro.
- `CopilotController` agora declara `Authorize` localmente, sem depender apenas da política fallback do host.
- O Totem publicado deixou de inventar `KIOSK-001`: o código do dispositivo deve vir da query string provisionada ou de `VITE_KIOSK_DEVICE_CODE`. Tenant e unidade continuam vindo de `BarberSync:DefaultTenantId` e `BarberSync:DefaultBranchId` no host API.

## Authenticated production-readiness contract

`POST /api/auth/login` returns the real JWT in `data.accessToken`; readiness clients send it as `Authorization: Bearer`. The smoke verifies an invalid login has `traceId`, then exercises `/api/dashboard/summary`, role-scoped `/api/mobile/summary`, persistent notification read-all, stock and current cash-register data. Client responses must omit professional commissions/blocks; professional responses must include those owned collections. Kiosk discovery requires the explicit `READINESS-KIOSK-001` query value and omission returns 400. No 404 or 500 is accepted.

## Production-readiness POS contract (Sprint 26)

The canonical flow is `POST /api/service-orders/open`, `GET /api/service-orders/{id}`,
`POST /api/service-orders/{id}/items/services`, `POST /api/service-orders/{id}/items/products`,
`DELETE /api/service-orders/{id}/items/{itemId}` (JSON body `{ "reason": "..." }`) and
`POST /api/service-orders/{id}/payments`. Opening uses `{clientId, appointmentId?, notes?}`;
service/product items use catalog IDs plus quantity and the service professional; payment uses
`{idempotencyKey, splits:[{method,amount,receivedAmount?}], note?}`. Read-back evidence uses
`GET /api/cash-registers/current`, `/api/stock`, `/api/stock/movements`, `/api/finance`,
`/api/commissions`, and `/api/audit`. All are authenticated and tenant/branch scoped. A paid
order must expose status `Paid` and zero balance; its payment ID/order ID correlate the cash,
revenue, stock, commission and audit records. The readiness total is 65.00 (service 40.00 plus
product 25.00).

### Correlação POS de estoque e caixa

`GET /api/stock/movements` é autenticado, exige `Stock.View` e aplica o tenant/branch dos claims. Cada movimento inclui `id`, `tenantId`, `branchId`, `productId`, `serviceOrderId`, `type`, `quantity`, `balanceAfter`, `reason`, `status` e `createdAt`. Em `GET /api/cash-registers/current`, cada item de `movements` inclui `paymentId` anulável (`Guid?`): movimentos manuais ou históricos podem não estar associados a pagamento, enquanto vendas POS devem carregar a correlação. A leitura dos movimentos de caixa reaplica `tenant_id` e `branch_id` além do identificador do caixa, mantendo a fronteira multi-tenant também na consulta filha.
# Relacionamento (Sprint 35)

| Método | Rota | Contrato |
|---|---|---|
| GET | `/api/relationship/dashboard` | KPIs do tenant/unidade autenticados. |
| GET/PUT | `/api/clients/{id}/profile` | Perfil 360 real e permissões de cliente. |
| GET | `/api/clients/{id}/timeline` | Agendamentos, comandas e fidelidade. |
| GET | `/api/relationship/segments` | Segmentos determinísticos. |
| GET | `/api/relationship/segments/{key}/clients` | Clientes calculados na unidade. |
| GET/POST | `/api/relationship/campaigns` | Campanhas internas, sem envio externo. |
| GET/POST/PUT | `/api/packages` | Catálogo de pacotes. |
| POST | `/api/client-packages/sell` | Venda transacional. |
| POST | `/api/client-packages/{id}/use` | Consumo com vigência e saldo. |
| GET/POST/PUT | `/api/coupons` | Administração de cupons. |
| POST | `/api/service-orders/{id}/coupon` | Valida e aplica cupom. |
| POST | `/api/service-orders/{id}/cashback` | Resgata cashback. |
| GET | `/api/mobile/summary` | Benefícios com ownership do cliente. |

# Equipe/RH (Sprint 36)

Todas as rotas abaixo exigem JWT, permissão `Professional.Read` ou `Professional.Update` e usam exclusivamente `tenant_id`/`branch_id` dos claims. Erros retornam `traceId`.

| Método | Rota | Contrato |
|---|---|---|
| GET | `/api/team/dashboard` | KPIs reais de profissionais, faturamento, comissões e metas da unidade. |
| GET | `/api/professionals` e `/{id}` | Cadastro-base ativo e tenant-scoped. |
| GET/PUT | `/api/professionals/{id}/profile` | Perfil, vínculo, especialidades e status. |
| GET/POST/PUT | `/api/professionals/{id}/services[/{serviceId}]` | Serviços, preço/duração e comissão sobrescritos. |
| GET/PUT | `/api/professionals/{id}/schedule` | Escala semanal, pausa e vigência. |
| GET/POST/PUT | `/api/professionals/{id}/time-off[/{timeOffId}]` | Bloqueio, folga, férias e indisponibilidade. |
| GET | `/api/professionals/{id}/performance` | Produção calculada de agenda, comandas e comissões. |
| GET/POST/PUT | `/api/commissions/rules[/{id}]` | Regras percentuais/fixas por profissional, serviço, produto ou pacote. |
| GET/POST | `/api/commissions/settlements` | Consulta e fechamento atômico das comissões abertas. |
| POST | `/api/commissions/settlements/{id}/approve` | Aprova settlement fechado. |
| POST | `/api/commissions/settlements/{id}/mark-paid` | Paga settlement aprovado e cria payout. |
| GET/POST/PUT | `/api/professional-goals[/{id}]` | Meta por tipo/período e progresso. |
| GET/POST | `/api/professional-payouts` | Extrato e pagamento por settlement. |
| GET | `/api/mobile/summary` | Para `Professional`, somente agenda, produção, comissões, metas, ocupação e indisponibilidades próprias. |

## Financeiro avançado

Todos exigem autenticação, contexto de tenant/filial e permissões financeiras: `GET /api/finance/dashboard`, `GET|POST|PUT /api/finance/categories`, `GET|POST|PUT /api/suppliers`, CRUD e ações em `/api/finance/payables` e `/api/finance/receivables`, regras em `/api/finance/recurring-rules`, conciliações em `/api/finance/reconciliations`, `GET /api/finance/cash-flow`, `GET /api/finance/dre` e `GET /api/finance/reports/export?type=&from=&to=`. Erros de validação retornam `message`, `traceId` e `errors` por campo. Veja [FINANCE_WORKFLOW.md](FINANCE_WORKFLOW.md).

## Estoque avançado (Sprint 38)

Todos os contratos exigem autenticação, escopo tenant/filial e permissões `Inventory.*`: `GET /api/inventory/dashboard`, produtos, categorias, pedidos (aprovar/cancelar), recebimentos (postar), contagens (fechar), transferências (enviar/receber), fichas de insumos, reposição (converter/descartar) e `GET /api/inventory/reports/export`. POST/PUT retornam erros por campo e `traceId`.

## BI Executivo / Analytics

Todos os contratos exigem autenticação, claims `tenant_id`/`branch_id` e a permissão indicada. `GET /api/analytics/{executive|operations|finance|team|relationship|inventory}` (`Analytics.Read`) retorna `period`, indicadores de fontes canônicas, `sourceStatus` e, no executivo, comparação com período anterior. `GET /api/analytics/kpis`, `/rankings` e `/filter-options` complementam dashboards sem aceitar IDs digitados. Alertas usam `GET /api/analytics/alerts`, `POST|PUT /api/analytics/alerts/rules[/{id}]` e `POST /api/analytics/alerts/{id}/{acknowledge|resolve|dismiss}` (`Analytics.Alerts`). Visões usam CRUD em `/api/analytics/saved-views` (`Analytics.Read`/`Analytics.Manage`). `GET /api/analytics/reports/export?type=...&from=...&to=...` (`Analytics.Export`) entrega CSV e registra a exportação. Erros de filtro retornam `message`, `traceId` e `errors` por campo.
# Comunicação (Sprint 40)

Todos os contratos abaixo são autenticados, usam escopo das claims e retornam erro com `traceId`: `GET /api/communication/dashboard`, canais, templates, campanhas, automações, outbox, retry/cancel, `GET|POST /api/notifications/inbox`, `GET|PUT /api/notifications/preferences` e `GET /api/communication/reports/export`. Consulte [COMMUNICATION_WORKFLOW.md](COMMUNICATION_WORKFLOW.md) para consentimento e estados de entrega.

## IA Operacional (Sprint 41)

Contratos autenticados e tenant/branch-scoped: `GET /api/ai-operations/dashboard`, CRUD versionado de `cameras`, `zones` e `signal-rules`, `POST /detection-events`, `GET /review-queue`, `GET /suggestions`, ações `approve|correct|reject`, evidências, configurações e `GET /reports/export?type=&from=&to=`. Sugestões sempre exigem revisão humana; aprovação adiciona item à comanda, mas não confirma pagamento. Provider ausente retorna `ProviderNotConfigured` sem sucesso simulado. Veja [AI_OPERATIONS_WORKFLOW.md](AI_OPERATIONS_WORKFLOW.md).

## Governança SaaS
Todas as rotas abaixo são autenticadas, usam o escopo dos claims e retornam falhas com `traceId`: `GET /api/governance/dashboard|tenants|branches|users|roles|permissions/matrix|plans|subscription|modules|security/policies|security/events|privacy/requests|onboarding|exports`; `POST /branches|users/invite|roles|privacy/requests|exports`; `PUT /branches/{id}|roles/{id}|permissions/matrix|modules/{moduleKey}`; e os comandos de usuário, privacidade e onboarding. Limite retorna HTTP 409, `upgradeRequired: true` e `errors.plan`.

## Integração Clube & Vendas
Consulte `CLUB_AND_SALES_WORKFLOW.md` para contratos de assinatura, carteira, resgate, venda pendente, auditoria e regras de origem.

### Clube & Vendas (`/api/club`)
Todos os contratos administrativos exigem autenticação, escopo de tenant/unidade e a permissão `Club.*` correspondente. Dashboard e opções usam `GET /dashboard` e `GET /filter-options`. Planos usam `GET|POST /plans`, `PUT /plans/{id}`, e transições `activate|archive`. Assinaturas usam `GET|POST /memberships`, transições `activate|suspend|cancel` e `GET /memberships/{id}/usage`. Carteiras usam `GET /wallets`, `GET /wallets/{clientId}` e mutações `credit|debit|adjust`. Gift cards usam `GET|POST /gift-cards`, `activate|cancel` e `POST /gift-cards/redeem`. Catálogos incluem `/vouchers`, `/combos` e `/online-sales`; exportação exige `Club.Reports.Export` em `GET /reports/export`. Erros retornam ProblemDetails com `traceId` e falhas de regra não produzem sucesso simulado.

Contratos Mobile: `GET /api/mobile/club/summary|wallet|memberships|gift-cards|vouchers` e `POST /api/mobile/club/vouchers/redeem`. Códigos são dados comerciais informados pelo cliente, não IDs técnicos.

## Integração — Portal do Cliente (Sprint 51)

O fluxo client-scoped, seus limites de privacidade, eventos e comportamento sem provider estão documentados em [CLIENT_PORTAL_WORKFLOW.md](CLIENT_PORTAL_WORKFLOW.md). A integração não aceita identificadores técnicos digitados e não transforma intenção de pagamento em liquidação.

## Integração Qualidade & Retenção — Sprint 52

O contrato de integração, escopo, eventos e restrições está em [QUALITY_AND_RETENTION_WORKFLOW.md](QUALITY_AND_RETENTION_WORKFLOW.md). Os dados permanecem tenant/branch scoped; indisponibilidade não produz resultado fictício, e nenhuma integração usa biometria ou inferência de emoção.

## Marketing Studio

Admin autenticado: `GET /api/marketing/dashboard`, CRUD e transições em `/segments`, `/campaigns`, `/journeys`, `/landing-pages`, `/promo-links`, `/qr-codes`, `/calendar`, `/experiments`, além de `GET /api/marketing/reports/export` e `/filter-options`. Erros incluem `traceId`; início externo sem provider retorna `409 ProviderNotConfigured`.

Público: `GET /api/marketing/public/landing/{slug}`, `GET /api/marketing/public/go/{publicSlug}` e `POST /api/marketing/public/track`. Mobile autenticado: `GET /api/mobile/marketing/offers`, `GET /api/mobile/marketing/campaigns` e `POST /api/mobile/marketing/track`.

## Marketplace & Parceiros

Admin autenticado: `GET /api/partners/dashboard|profiles|affiliates|links|referrals|commission-rules|commissions|payouts|contracts|marketplace-items|supplier-terms|filter-options`, seus comandos de criação/transição e `GET /api/partners/reports/export`. Público: `GET /api/partners/public/marketplace`, `GET /api/partners/public/partner/{slug}` e `POST /api/partners/public/track`. Mobile autenticado: `GET /api/mobile/partners/marketplace`, `GET /api/mobile/partners/offers` e `POST /api/mobile/partners/track`. Consulte `PARTNERS_MARKETPLACE_WORKFLOW.md` para invariantes de pagamento, comissão e privacidade.

## Central de Controle (`CommandCenter.Read` por padrão)

- `GET /api/command-center/dashboard`, `/executive`, `/operations`, `/health` — snapshots, operação e matriz de saúde reais.
- `GET /api/command-center/alerts`; `POST /alerts/{id}/acknowledge|resolve|dismiss` — ciclo de alertas.
- `GET|POST /api/command-center/incidents`; `PUT /incidents/{id}`; `POST /incidents/{id}/resolve` — incidentes.
- `GET|POST /api/command-center/tasks`; `PUT /tasks/{id}`; `POST /tasks/{id}/complete` — fila operacional.
- `GET /api/command-center/integrations`, `/filter-options`, `/reports/export` — integrações, opções tenant-scoped e CSV.

Erros usam `ProblemDetails` com `traceId`; fontes sem medição não são convertidas em `Healthy`.

## Sprint 57 — Catálogo & Precificação

Admin autenticado: `GET /api/catalog/dashboard`, `GET|POST|PUT /api/catalog/services[/profile|/{id}/profile]`, `GET|POST|PUT /api/catalog/products[/profile|/{id}/profile]`, CRUD e transições em `/api/catalog/combos`, `/packages`, `/pricing-rules`, CRUD em `/margin-rules` e `/commission-rules`, `POST /api/catalog/simulate-price`, `POST /api/catalog/simulate-commission`, `GET /api/catalog/audit`, `GET /api/catalog/reports/export` e `GET /api/catalog/filter-options`. Todos são tenant/branch scoped, protegidos por `Catalog.*`, ProblemDetails e traceId.

Mobile autenticado: `GET /api/mobile/catalog/services`, `GET /api/mobile/catalog/products`, `GET /api/mobile/catalog/combos` e `POST /api/mobile/catalog/simulate-price`. Listagens retornam apenas itens ativos, visíveis e comercialmente disponíveis.

## Sprint 58 · Atendimento 360

O contrato integrado, estados, transações e responsabilidades deste módulo estão documentados em [SERVICE_EXECUTION_CHECKOUT_WORKFLOW.md](SERVICE_EXECUTION_CHECKOUT_WORKFLOW.md). Eventos reais são correlacionados por `service_order_id`; preview não altera ledger e estados pendentes não são tratados como receita, consumo ou comissão paga.

### Atendimento 360 (`ServiceExecution.*`, autenticado e scoped)

- `GET /api/service-execution/dashboard|today|orders|orders/{id}|cashier/current|commissions|inventory-consumption|audit|filter-options`
- `POST /api/service-execution/check-in|orders/from-appointment|orders/{id}/services|orders/{id}/products|orders/{id}/start-service|orders/{id}/complete-service|orders/{id}/cancel-service`
- `POST /api/service-execution/checkout/preview|apply-discount|apply-benefit|create-payment-intent|confirm|cancel`
- `POST /api/service-execution/cashier/open|close|adjust|commissions/preview|commissions/reverse|inventory-consumption/preview|inventory-consumption/reverse`
- `GET /api/service-execution/reports/export`
- Mobile: `GET /api/mobile/service-execution/today|orders`; `POST /api/mobile/service-execution/check-in|orders/{id}/complete-service|checkout/preview`.

## Integração Sprint 60 — Equipe & RH 360

O contrato canônico e as regras de isolamento, disponibilidade, produtividade, comissão, qualidade, alertas e relatórios estão descritos em [TEAM360_WORKFLOW.md](TEAM360_WORKFLOW.md). A integração preserva a origem real e publica `sourceStatus` quando uma fonte não está disponível.

## Sprint 61 — Financeiro 360

A superfície autenticada `/api/finance360` inclui dashboard, filter-options, receivables (CRUD de estado e aging), payables (agendamento, baixa e aging), reconciliation (preview/reconcile/divergent/reverse), cash-flow (projection/realized/snapshot), DRE (snapshot/export), commissions, payroll, partner-payouts, delinquency, audit e reports. Mobile expõe somente leitura em `/api/mobile/finance360/{summary,receivables,payables,commissions,payroll}`. Todas as rotas exigem permissões `Finance360.*` e escopo dos claims.

## Integração Inventory360 — Sprint 62

O contrato canônico está em [INVENTORY360_WORKFLOW.md](INVENTORY360_WORKFLOW.md). Dados de estoque e CMV são tenant/branch scoped, derivados de movimentos reais e retornam `sourceStatus=Unavailable` quando a origem necessária não existir; integrações não podem fabricar saldo, compra ou custo.

### Inventory360

`GET /api/inventory360/dashboard`, `GET|POST /api/inventory360/products`, `PUT /api/inventory360/products/{id}`, `POST /api/inventory360/products/{id}/activate|suspend|archive`, `GET /api/inventory360/supplies`, `GET /api/inventory360/suppliers`, `GET|POST /api/inventory360/service-inputs`, `POST /api/inventory360/service-inputs/preview`, `GET /api/inventory360/stock`, `GET /api/inventory360/batches`, `POST /api/inventory360/stock/receive|consume|reserve|release-reservation|reverse`, `GET|POST /api/inventory360/purchases`, `POST /api/inventory360/purchases/{id}/approve|receive|cancel|return-to-supplier`, `GET /api/inventory360/counts`, `POST /api/inventory360/counts/open`, `POST /api/inventory360/counts/{id}/items|close|adjust`, `GET /api/inventory360/replenishment`, `POST /api/inventory360/replenishment/generate`, `POST /api/inventory360/replenishment/{id}/create-purchase`, `GET /api/inventory360/costing`, `GET /api/inventory360/audit`, `GET /api/inventory360/reports/export`, `GET /api/inventory360/filter-options`.

Mobile: `GET /api/mobile/inventory360/summary|products|stock|replenishment` e `POST /api/mobile/inventory360/counts/{id}/items`. Todos exigem JWT, permission e escopo obtido dos claims.
