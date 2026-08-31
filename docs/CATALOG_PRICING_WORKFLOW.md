# Catálogo & Precificação

## Objetivo e fonte de verdade

O módulo da Sprint 57 consolida o perfil comercial de serviços e produtos, combos, pacotes, regras de preço, margem e comissão. Valores monetários usam `decimal` na API e `numeric` no PostgreSQL. Todo acesso administrativo exige autenticação, permissão, `tenant_id` e `branch_id`; o cliente nunca informa o escopo.

## Fluxo operacional

1. O gestor seleciona serviço/produto real em `/Catalog/Services` ou `/Catalog/Products`, registra custo, preço, duração/estoque e visibilidade.
2. Combos e pacotes são montados com seletores. Quantidade deve ser positiva; combo ativo exige item; pacote explicita validade e nunca admite saldo negativo.
3. Regras ativas e dentro da vigência são ordenadas por prioridade. O breakdown registra base, regras, desconto, acréscimo, preço final, custo, margem, alertas, aprovação e status da fonte.
4. Margem abaixo do mínimo gera `Warn`, `RequireApproval` ou `Block`. Preço/custo negativos e desconto que reduz preço abaixo de zero são rejeitados/limitados.
5. Comissão é apenas previsão até `ServiceCompleted`, `ProductSold`, `OrderPaid` ou `MembershipPaid`. A origem possui chave única; cancelamento/estorno deve produzir reversão, nunca segundo evento.
6. Simulações são persistidas em `catalog_price_simulations`; mudanças de preço e decisões operacionais permanecem em histórico/auditoria.

## Integrações

- **Agenda/PublicWeb/Mobile:** somente serviço `Active`, publicamente visível, com duração positiva e preço calculado.
- **PDV/Comanda:** deve consumir o breakdown central e impedir a reaplicação do mesmo voucher/cupom/clube.
- **Estoque:** produto controlado exige saldo; receita de insumo vincula custo do serviço e baixa apenas no evento operacional configurado.
- **Clube/Vendas:** benefício nasce depois da confirmação real do pagamento; validade e saldo são obrigatórios.
- **Marketplace/Kiosk:** exigem visibilidade específica e item ativo; o Kiosk mantém `Kiosk:DeviceCode` configurado, sem query string.
- **Financeiro/Equipe/Parceiros:** comissão somente fica pagável após origem real; `Paid` depende do fluxo de payout/pagamento confirmado.
- **BI:** margens médias, itens abaixo do mínimo, rentabilidade, descontos, comissões e receita por categoria são derivados das tabelas do catálogo.

## Contrato de segurança e UX

Nenhum formulário pede UUID/ID técnico digitável. IDs aparecem apenas em valores de opções obtidas de `/api/catalog/filter-options`. Erros usam ProblemDetails e `traceId`; estados vazios não fabricam registros e falhas não retornam sucesso. Seeds são evidências controladas, idempotentes, e não simulam pagamento nem comissão paga.

## Sprint 58 · Atendimento 360

O contrato integrado, estados, transações e responsabilidades deste módulo estão documentados em [SERVICE_EXECUTION_CHECKOUT_WORKFLOW.md](SERVICE_EXECUTION_CHECKOUT_WORKFLOW.md). Eventos reais são correlacionados por `service_order_id`; preview não altera ledger e estados pendentes não são tratados como receita, consumo ou comissão paga.
