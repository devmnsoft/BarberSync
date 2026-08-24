# Contratos de rotas da API — Sprint de Produção 24

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
| Appointments | POST | `/api/appointments/{id}/cancel` | Appointments/Cancel | Sim | `Appointment.Cancel` | Admin | motivo no DTO |
| Appointments | GET | `/api/appointments/smart-slots` | Appointments/SmartSlots | Sim | `Appointment.Read` | Admin | OK |
| Clients | CRUD | `/api/clients[/{id}]` | Clients | Sim (fallback) | sessão | Admin | OK |
| Professionals | CRUD | `/api/professionals[/{id}]` | Professionals | Sim (fallback) | sessão | Admin | OK |
| Services | CRUD | `/api/services[/{id}]` | Services | Sim (fallback) | sessão | Admin | OK |
| Products | CRUD | `/api/products[/{id}]` | Products | Sim (fallback) | sessão | Admin | OK |
| Stock | GET/POST | `/api/stock`, `/api/stock/movements`, `/api/stock/{entry\|exit\|adjustment}` | Stock | Sim (`Authorize`) | `Stock.*` | Admin | produto e movimentos reais |
| Purchases | CRUD/POST | `/api/purchases[/{id}]`, `/{id}/receive` | Purchases | Sim (`Authorize`) | Manager+ na escrita | Admin | OK |
| ServiceOrders | POST | `/api/service-orders/open` | ServiceOrders/Open | Sim (`Authorize`) | `ServiceOrder.Create` | Admin | OK |
| ServiceOrders | POST/DELETE | `/api/service-orders/{id}/items/{services\|products\|{itemId}}` | ServiceOrders items | Sim | `ServiceOrder.Update` | Admin | motivo exigido na remoção |
| Payments | POST | `/api/service-orders/{id}/payments` | ServiceOrders/Pay | Sim | `Payment.Create` | Admin PDV | contrato canônico |
| Payments | CRUD | `/api/payments[/{id}]` | Payments | Sim (`Authorize`) | sessão | Admin | Proteção explícita corrigida |
| CashRegisters | GET/POST | `/api/cash-registers/current`, `/open`, `/{id}/{supply\|withdrawal\|expense\|close}` | CashRegisters | Sim (`Authorize`) | `Cash.*` | Admin | OK |
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
