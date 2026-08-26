# Estoque avançado e compras

## Escopo e segurança
O módulo Admin `/Inventory` usa exclusivamente a API relacional autenticada. Consultas são isoladas pelo `tenant_id` e `branch_id` da sessão e exigem permissões `Inventory.Read`, `Inventory.Manage`, `Inventory.Purchase`, `Inventory.Receive`, `Inventory.Count`, `Inventory.Transfer` ou `Inventory.Export`. Erros retornam mensagem, campos e `traceId`; não existe sucesso alternativo.

## Produtos, categorias e fornecedores
Produtos possuem categoria, uso (`Sale`, `Input` ou `Both`), unidade, custo, preço, estoque mínimo e ponto de reposição. Categoria e fornecedor principal são escolhidos em listas da API; IDs ficam somente no valor interno da opção. O ponto de reposição nunca pode ser inferior ao mínimo e o fornecedor deve estar ativo na filial.

## Compra, recebimento e financeiro
O pedido nasce em rascunho com fornecedor, categoria financeira e produtos selecionados. Aprovação exige itens. Recebimentos aceitam pedidos aprovados, podem ser parciais e não ultrapassam o saldo pendente. A postagem registra movimento, atualiza produto e pedido e, quando solicitado, cria conta a pagar única por recebimento. Lote e validade são opcionais.

## Inventário, ajustes, perdas e transferências
A contagem preserva o saldo do sistema e calcula divergência. A observação auditável é obrigatória; o fechamento gera ajuste, movimento e saldo. Transferências só ocorrem entre filiais diferentes do tenant: envio baixa a origem e recebimento único credita o destino. Ajustes, perdas e cancelamentos exigem motivo.

## Reposição e ficha técnica de serviços
Sugestões surgem quando o estoque alcança o ponto de reposição. Converter cria pedido real em rascunho; descartar exige motivo. Fichas aceitam produtos `Input` ou `Both`, sem repetição. Ao pagar a comanda, `PostSale` registra `ServiceConsumption`; pedido/produto/tipo impede repetição e produto vendido explicitamente não é baixado novamente como insumo.

## PDV, Operação, formulários e relatórios
O PDV bloqueia venda e consumo sem saldo. Dashboard e Operação consomem indicadores de críticos, compras, contagens, transferências, reposição e lotes. Formulários usam `form-validation.js`, tipos numéricos/data, loading e bloqueio de reenvio. Nenhuma tela pede UUID: entidades são seleções reais. CSV exige tipo e período válido.

## Limitações conhecidas
O crédito de transferência requer produto compatível já cadastrado na filial destino, conforme o contrato multi-filial existente; não há criação implícita ou resultado fictício. Recebimentos são sempre orientados pelos itens pendentes do pedido.

## Integração com BI

Os indicadores gerenciais deste domínio são agregados pelo módulo descrito em `docs/ANALYTICS_WORKFLOW.md`, sem duplicar a fonte operacional canônica.
