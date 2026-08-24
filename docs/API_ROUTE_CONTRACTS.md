# Contratos de rotas da API — Sprint de Produção 23

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
| Stock | GET/POST | `/api/stock`, `/api/stock/{entry\|exit\|adjustment}` | Stock | Sim (`Authorize`) | `Stock.*` | Admin | OK |
| Purchases | CRUD/POST | `/api/purchases[/{id}]`, `/{id}/receive` | Purchases | Sim (`Authorize`) | Manager+ na escrita | Admin | OK |
| ServiceOrders | POST | `/api/service-orders/open` | ServiceOrders/Open | Sim (`Authorize`) | `ServiceOrder.Create` | Admin | OK |
| ServiceOrders | POST/DELETE | `/api/service-orders/{id}/items/{services\|products\|{itemId}}` | ServiceOrders items | Sim | `ServiceOrder.Update` | Admin | motivo exigido na remoção |
| Payments | POST | `/api/service-orders/{id}/payments` | ServiceOrders/Pay | Sim | `Payment.Create` | Admin PDV | contrato canônico |
| Payments | CRUD | `/api/payments[/{id}]` | Payments | Sim (`Authorize`) | sessão | Admin | Proteção explícita corrigida |
| CashRegisters | GET/POST | `/api/cash-registers/current`, `/open`, `/{id}/{supply\|withdrawal\|expense\|close}` | CashRegisters | Sim (`Authorize`) | `Cash.*` | Admin | OK |
| Financial | GET | `/api/finance` | Finance/GetAll | Sim (`Authorize`) | sessão | Admin | OK |
| Commissions | GET | `/api/commissions` | Commissions/GetAll | Sim (`Authorize`) | Professional/Manager+ | Admin/Mobile profissional | OK |
| Reports | GET | `/api/executive/{owner\|reception\|export.csv}` | Executive | Sim (`Authorize`) | roles por action | Admin | período compartilhado |
| Notifications | GET/POST | `/api/notifications`, `/{id}/read`, `/read-all` | Notifications | Sim (`Authorize`) | sessão | Admin | Proteção explícita corrigida |
| Audit | GET | `/api/audit`, `/events`, `/{id}` | Audit | Sim (`Authorize`) | sessão | Admin | Proteção explícita corrigida |
| ServiceRecognition | GET/POST | `/api/service-recognition/*` | ServiceRecognition | Sim (`Authorize`) | Owner a Reception | Admin | confirmação humana |
| AiSettings | GET/POST | `/api/system/ai-settings`, `/test` | AiSettings | Sim (`Authorize`) | Owner/Admin | Admin | OK |
| Copilot | GET/POST | `/api/copilot/{conversations\|messages\|ask\|suggestions\|actions\|feedback}` | Copilot | Sim (fallback) | tenant da sessão | Admin | isolamento preservado |
| Totem/Public | GET/POST | `/api/kiosk/*` | Kiosk | Pública controlada | device + escopo configurado | Totem | justificada |
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
