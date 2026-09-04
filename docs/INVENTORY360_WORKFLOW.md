# Estoque & Compras 360

## Fonte de verdade

Inventory360 é isolado por `tenant_id` e `branch_id`. Produtos, insumos, fornecedores e regras são cadastros; saldo só muda por movimentos imutáveis com origem e chave de idempotência. Não há fallback demonstrativo. Ausência de custo retorna `sourceStatus=Unavailable`.

## Ciclo operacional

1. Produto ou insumo ativo é selecionado (IDs internos nunca são digitados).
2. Compra nasce `Draft`, exige permissão para aprovação e aceita recebimentos parciais.
3. O recebimento confirmado adiciona movimento `Receive`, atualiza custo médio e, se solicitado, cria payable no Financeiro 360 com a origem do recebimento.
4. Venda confirmada e execução do serviço consomem somente regras ativas. Reserva, liberação, perda, devolução e estorno criam novos movimentos; nenhum ledger é editado ou apagado.
5. Lotes elegíveis são consumidos por FEFO; vencidos/bloqueados são indisponíveis. Transferência cria saída e entrada correlacionadas nas filiais.
6. Inventário registra esperado e contado. Divergência requer motivo e o ajuste gera movimento auditável.
7. Reposição compara mínimo/alvo com saldo persistido. Histórico insuficiente é explícito e uma sugestão só vira compra após confirmação.

## Custo e integrações

CMV usa custo de lote quando presente e custo médio caso contrário. Sem custo, não produz zero fictício. Atendimento 360 deve chamar consumo em `ServiceStarted`, `ServiceCompleted` ou `CheckoutConfirmed`; estornos chamam reversão. Catálogo, PublicWeb, Marketplace e Kiosk só disponibilizam item ativo/visível e, havendo controle, com saldo. DRE e BI consomem CMV real. Command Center observa `StockBelowMinimum`, `BatchExpiring`, `BatchExpired`, `PurchaseOrderApproved`, `PurchaseOrderReceived`, `InventoryCountDivergent`, `StockMovementReversed` e `SupplierReturnConfirmed`. Até existir barramento Workflow Studio, a publicação permanece `sourceStatus=Unavailable` sem sucesso simulado.

## Segurança e falhas

Todas as rotas são autenticadas e autorizadas por permissão. O contexto vem dos claims, nunca do payload. Validação rejeita quantidade não positiva, item ambíguo, origem ausente, excesso de recebimento/transferência, lote vencido e saldo negativo. Erros seguem ProblemDetails global e incluem `traceId`.
