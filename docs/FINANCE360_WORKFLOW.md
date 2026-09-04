# Financeiro 360 — workflow definitivo

## Fonte de verdade e escopo

Todas as consultas e mutações usam `tenant_id` e `branch_id` obtidos dos claims autenticados. Valores monetários são `decimal` na API e `numeric(14,2)` no PostgreSQL. A interface nunca solicita identificadores técnicos: opções são carregadas por `/api/finance360/filter-options`.

## Lançamentos e baixa

`FinancialPostingService` mantém um razão append-only e idempotente por origem. Checkout, pagamento, comissão, folha e payout criam lançamentos apenas a partir de uma origem persistida. Uma reversão cria o crédito/débito contrário e preserva o original. Recebíveis e pagáveis nascem abertos; pagamento parcial acumula `paid_amount`, e pagamento total só é aceito com `payment_id` confirmado ou baixa manual autorizada acompanhada de motivo e auditoria.

## Conciliação

Preview é somente leitura. `Matched` requer payment `Confirmed`/`Paid`, posting confirmado e diferença dentro da tolerância. Divergência exige motivo. Reversão cria novo evento e não apaga o registro conciliado.

## Fluxo de caixa e DRE

O projetado soma saldos abertos de receivables/payables; o realizado soma somente postings confirmados. A DRE deriva receita e despesas do razão, sem preencher fontes ausentes. Snapshots preservam o resultado calculado e exports CSV usam o mesmo período obrigatório.

## Inadimplência e integrações

Recebíveis vencidos podem originar casos únicos de inadimplência; atribuição, negociação e encerramento são auditados. Atendimento cria a origem de checkout/payment, Team360 e Parceiros geram pagáveis aprovados, Catálogo/Estoque fornecem custo e desconto, e Command Center/BI consomem alertas e métricas persistidas. Contratos Mobile são somente leitura e respeitam as mesmas permissões.

## Integração Inventory360 — Sprint 62

O contrato canônico está em [INVENTORY360_WORKFLOW.md](INVENTORY360_WORKFLOW.md). Dados de estoque e CMV são tenant/branch scoped, derivados de movimentos reais e retornam `sourceStatus=Unavailable` quando a origem necessária não existir; integrações não podem fabricar saldo, compra ou custo.
