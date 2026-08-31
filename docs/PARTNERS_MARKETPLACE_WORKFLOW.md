# Marketplace & Parceiros

## Escopo e segurança

O módulo é sempre isolado por `tenant_id` e `branch_id`. O Admin exige autenticação e permissões `Partners.*`; slugs públicos não expõem UUIDs. O código do afiliado é gerado com entropia criptográfica, persistido somente como SHA-256 e apresentado de forma mascarada.

## Fluxos

1. Um parceiro nasce em `Draft`. Ativar, suspender ou arquivar exige motivo; o histórico é preservado.
2. Somente parceiro `Active` cria afiliado ou link. Links apontam para Booking, Store, Landing, Portal, Review ou Marketplace reais.
3. Indicações são capturadas sem converter automaticamente o lead e têm deduplicação curta por contato, parceiro e origem. Perda exige motivo.
4. Regras cobrem lead qualificado, atendimento concluído, pedido/pagamento e clube. A comissão tem origem única rastreável. `Payable` exige o evento real correspondente; pendências e cancelamentos não são pagamentos.
5. Payout nasce `Draft`, aprovação não representa pagamento e `Paid` requer a seleção de um pagamento confirmado. Comissões pagas são somente estornadas, nunca apagadas.
6. O marketplace retorna apenas itens `Active`, sem custo/estoque interno. CTAs seguem para Booking, Store ou Portal; nenhuma compra é simulada.
7. Termos comerciais conectam parceiro a fornecedor e produto existentes, preço, lead time e MOQ. Não geram compra automática.

## Integrações

Tracking público grava `marketing_tracking_events` com janela de deduplicação. Referências de agendamento, venda, comanda, pagamento, clube, fornecedor e produto são validadas contra dados reais. O dashboard e CSV oferecem KPIs atribuídos; Mobile consome somente ofertas ativas. Quando uma fonte não puder ser consultada, consumidores devem apresentar `sourceStatus: unavailable`, nunca valores inventados.

## Operação e LGPD

Dados de contato aparecem apenas no Admin autorizado. A vitrine expõe nome, descrição e preço público. Erros seguem ProblemDetails com `traceId`. O seed de readiness é determinístico, identificado e não marca payout como pago.

## Integração Sprint 57 — Catálogo & Precificação

A operação consome a fonte central de preço, custo, margem, duração, visibilidade e breakdown descrita em [CATALOG_PRICING_WORKFLOW.md](CATALOG_PRICING_WORKFLOW.md). Benefícios e comissões permanecem pendentes até o evento comercial real; escopo de tenant/unidade e trilha de auditoria são obrigatórios.
