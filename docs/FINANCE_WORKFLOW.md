# Financeiro avançado

O módulo `/Finance` é o razão operacional financeiro por tenant e filial. Todos os endpoints exigem autenticação e permissões `Finance.Read`, `Finance.Manage` ou `Finance.Export`; o contexto autenticado fornece `tenant_id` e `branch_id` e toda consulta aplica ambos.

## Cadastros e lançamentos

Categorias classificam receitas, despesas, transferências e ajustes e podem formar hierarquia. Fornecedores guardam contato, documento e endereço. Contas a pagar e receber começam em `Open`, podem passar a `Overdue` e terminam como `Paid`/`Received` ou `Cancelled`. Pagamento, recebimento e cancelamento só aceitam contas abertas/vencidas; cancelamento exige motivo.

A interface nunca solicita UUID: categoria, fornecedor e cliente são opções carregadas da API. Identificadores existem apenas no valor da opção/payload. Campos obrigatórios, e-mail, valor positivo e intervalos de datas são validados no navegador e novamente na API. O submit fica bloqueado durante a chamada e erros exibem o `traceId`.

## Recorrência e conciliação

Regras semanais, mensais e anuais guardam a próxima execução. A execução explícita gera lançamento real e avança a data. Conciliações selecionam um caixa da filial, registram período, esperado, realizado e diferença; apenas rascunhos podem ser fechados.

## Fluxo de caixa, DRE e relatórios

O fluxo agrega vencimentos de pagáveis e recebíveis. A DRE simplificada compara recebimentos e pagamentos realizados. A exportação CSV exige tipo selecionado e período válido e usa UTF-8. As opções incluem fluxo, DRE, contas por status, categorias, meios de pagamento, fornecedores, inadimplência e conciliações.

## Integrações e efeitos colaterais

Pagamentos à vista continuam originados no PDV, movimentos no caixa continuam no contrato de caixa e referências de comanda/pagamento/movimento ficam disponíveis nos recebíveis. Compras podem referenciar fornecedor, categoria e `source_purchase_id`; ao receber uma compra, estoque e conta a pagar devem ser gravados na mesma operação transacional pelo fluxo de compras. Receber uma conta deve registrar a referência do movimento de caixa.

## Auditoria e limites conhecidos

Criação/alteração, liquidação, cancelamento, recorrência, conciliação e exportação usam o contrato de auditoria existente. Não há execução automática de recorrências nesta sprint: ela é disparada pelo endpoint autenticado. Conciliação não modifica movimentos históricos; registra apenas a comparação e seu fechamento.
