# Atendimento 360 — execução, checkout e caixa

## Escopo e invariantes

O fluxo autenticado e tenant/branch scoped é `Agenda → Check-in → Comanda → Execução → Checkout → Pagamento confirmado → Estoque → Comissão → Caixa → Timeline/BI`. IDs vêm de seletores carregados por `/api/service-execution/filter-options`; nenhum operador digita UUIDs.

- Check-in aceita apenas agenda `Scheduled`/`Confirmed`, grava horário/atraso e é serializável. Cancelamento, no-show e check-in repetido falham sem sucesso alternativo. No Kiosk, `DeviceCode` continua obrigatório pela fronteira existente.
- A comanda só recebe item em `Open`. Cada item consulta perfil ativo do `CatalogPricingService`, persiste breakdown e custo em snapshot e não é recalculado silenciosamente.
- A prévia do checkout é somente leitura (`mutated=false`). Desconto + benefício nunca excedem subtotal. Um benefício declara origem (`Voucher`, `Coupon`, `GiftCard`, `Wallet`, `Cashback`, `Package` ou `Membership`) e uma sessão ativa única impede reaplicação.
- Criar intenção persiste pagamento `Pending`; não confirma gateway. Somente pagamento externamente `Confirmed` pode receber allocation e fechar checkout/comanda em transação. Reversão preserva allocation original e reabre `PendingPayment`.
- Consumo bloqueia saldo insuficiente quando o produto não permite negativo, atualiza saldo, cria movimento e snapshot na mesma transação. Estorno acrescenta movimento reverso; nada é apagado.
- Comissão usa `CatalogCommissionService`, exige evento concluído e `idempotency_key` única. Nasce `Payable`, nunca `Paid`. Registros pagos não são editados pelo fluxo operacional.
- Um operador abre apenas um caixa por filial. Caixa fechado não recebe movimento; ajuste exige motivo; divergência no fechamento exige justificativa.

## Estados

`Appointment: Scheduled|Confirmed → CheckedIn`; `ServiceOrder: Open → InService → ServiceCompleted → Paid → Closed` (ou `Cancelled`); `Checkout: Draft → PendingPayment|Paid|Cancelled|Failed`; `Allocation: Pending|Confirmed|Reversed|Cancelled`; `Consumption: Reserved|Consumed|Reversed|Cancelled`; `Commission: Pending|Payable|Paid|Reversed|Cancelled`; `CashSession: Open|Closed|Cancelled`.

## Transação e auditoria

Operações monetárias usam `decimal`/`numeric`. Check-in, alterações de estoque, confirmação/reversão e mudanças compostas usam transação, com isolamento serializável nos pontos de concorrência. `service_execution_events`, `service_order_audit_events`, snapshots e movimentos são append-only. Erros passam pelo ProblemDetails global e incluem `traceId`.

## Integrações e sourceStatus

Agenda é a origem do check-in; Catálogo, do preço/margem/comissão; Cliente 360 recebe `ServiceCompleted`; Financeiro fornece o estado real do pagamento; Estoque mantém movimentos; Parceiros permanecem `Payable`; relatórios/BI leem os mesmos snapshots. Eventos `ServiceOrderOpened`, `ServiceStarted`, `ServiceCompleted`, `CheckoutStarted`, `CheckoutCompleted`, `PaymentPending`, `PaymentConfirmed`, `InventoryConsumed`, `CommissionAccrued` e `CashSessionClosed` ficam persistidos para consumo pelo Workflow Studio. Quando um consumidor não está instalado, o `sourceStatus` é **persisted-awaiting-consumer**, sem descarte ou fallback.

Command Center pode derivar alertas de comandas abertas antigas, `PendingPayment`, divergência de caixa, bloqueio de estoque e conflito de idempotência. Qualidade/Marketing só tratam conversão após evento persistido real. PublicWeb/Portal consulta o pagamento real; não expõe a comanda administrativa.

## API e interface

As rotas estão em `docs/API_ROUTE_CONTRACTS.md`. O Admin `/ServiceExecution` oferece dashboard, fila, comandas, cadeiras, wizard de checkout, caixa, consumo, comissão e timeline. Empty/loading/error são explícitos, o erro mostra `traceId`, e layouts se adaptam a tablet e celular.

## Readiness

O seed cria apenas uma comanda aberta, evento, snapshot, checkout cancelado e auditoria vinculados a registros canônicos existentes. Não cria allocation confirmada, pagamento confirmado, baixa de estoque ou comissão. Assim, readiness cobre schema sem fabricar evento de negócio.
