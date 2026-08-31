# Operação integrada — Agenda, Atendimento, Comanda, PDV e Caixa

## Jornada canônica

O ponto de entrada do atendente é `GET /Operation/Today`. A página consulta, sempre pelo
gateway autenticado `/AdminApi`, a agenda do dia, comandas, profissionais e caixa atual. O
gateway apenas encaminha a requisição e a credencial para a API; ele não fabrica dados nem
respostas de sucesso. As rotas antigas `/Admin/Operations` e `/Admin/Attendance` redirecionam
para essa área, evitando duas telas para o mesmo processo.

1. **Agenda:** `GET /api/appointments?from=&to=` retorna somente o tenant e a unidade dos
   claims. Profissional, status e período são filtros visuais sobre esse conjunto diário.
2. **Chegada:** `POST /api/appointments/{id}/check-in` aceita apenas `Scheduled` ou
   `Confirmed`. Um segundo check-in é rejeitado pelo estado persistido.
3. **Atendimento:** ao iniciar, a interface localiza a comanda vinculada ou chama
   `POST /api/service-orders/open` com o cliente e o agendamento; somente depois chama
   `POST /api/appointments/{id}/start`. Não há escolha implícita de cliente ou profissional.
4. **Comanda:** o PDV abre comandas avulsas ou vinculadas, exige seleção explícita do
   profissional para um serviço, adiciona serviços/produtos ativos e recalcula totais no
   servidor. Produto sem saldo suficiente é rejeitado. A remoção envia uma justificativa no
   corpo do `DELETE` e gera auditoria.
5. **Pagamento:** `POST /api/service-orders/{id}/payments` recebe chave de idempotência,
   formas e valores. O status exibido é relido da API; pagamento parcial permanece aberto e
   pagamento integral conclui a comanda.
6. **Caixa:** `GET /api/cash-registers/current` e `/history` mostram o turno, saldo inicial,
   entradas, saídas, saldo esperado e movimentos. Abertura, suprimento, sangria, despesa e
   fechamento usam os endpoints reais e suas permissões.

## Regras e efeitos colaterais

- Cancelamento e ausência exigem motivo. As transições e reagendamentos são registrados em
  `appointment_history`, com usuário, estado anterior/novo e unidade.
- Comanda paga não pode ter itens alterados. Serviço deve estar ativo e habilitado para o
  profissional escolhido; produto deve estar ativo e respeitar a política persistida de
  estoque negativo.
- O pagamento é transacional e idempotente. Ele persiste `payments`, cria o movimento de
  caixa com `payment_id`, a receita em `financial_entries`, a saída em `stock_movements` para
  cada produto e a comissão dos serviços/profissionais. A comissão só nasce no pagamento.
- Os efeitos usam `tenant_id` e `branch_id`. As constraints/chaves de idempotência impedem
  pagamento, baixa e comissão duplicados. Um estorno autorizado reverte efeitos correlatos
  e registra auditoria.
- O painel de caixa mostra erro quando não há turno aberto; ele não inventa saldo. O estoque
  mostrado no catálogo vem de `products` e é relido após pagamento/atualização do PDV.

## Permissões

Todas as páginas exigem sessão (`[Authorize]`). Na API, leitura e mutações exigem
respectivamente `Appointment.*`, `Attendance.*`, `ServiceOrder.*`, `Payment.*`, `Cash.*` e
`Stock.*`. Comissão geral permanece restrita aos papéis gerenciais; a visão profissional usa
um contrato com ownership. Tenant e unidade nunca são aceitos do formulário operacional:
eles vêm do `ICurrentUserContext` alimentado pelos claims.

## Erros esperados

Erros de validação preservam a mensagem de negócio (transição inválida, motivo ausente,
estoque insuficiente, caixa fechado ou total inválido). `401` orienta novo login, `403`
explica falta de permissão e `404` informa que o recurso não pertence à unidade ou não existe.
Falhas do gateway e da API exibem `traceId` do corpo ou `X-Trace-Id`; stack traces não são
renderizados. Em timeout de pagamento, o operador deve atualizar a comanda antes de repetir,
usando a mesma chave de idempotência quando aplicável.

## Limitações conhecidas

- A Operação do Dia coordena contratos existentes; edição detalhada da comanda, recibo,
  estorno e abertura/fechamento ficam nas telas especializadas de PDV e Caixa.
- A comissão estimada não é calculada no navegador. Apenas comissão persistida após o
  pagamento é fonte de verdade nas telas gerenciais.
- Execução com banco real e evidência autenticada continua pertencendo ao gate externo de
  production readiness. Nenhum modo alternativo, armazenamento de navegador ou dado de
  demonstração é usado por este fluxo.

## Benefícios no fechamento

Antes do pagamento, o PDV pode aplicar cupom validado, consumir sessão de pacote ou resgatar cashback. O fechamento mantém essas alterações e o acúmulo de fidelidade no contrato transacional do pagamento. Consulte `docs/RELATIONSHIP_WORKFLOW.md` para vigência, saldo, ownership e auditoria.

## Integração com Equipe

A seleção do atendimento consulta a escala e os bloqueios persistidos do profissional, mostra alerta de indisponibilidade, ocupação e link para o perfil 360. A estimativa exibida usa o serviço vinculado; a comissão definitiva continua surgindo apenas na confirmação transacional do pagamento. A agenda rejeita profissional inativo, serviço não vinculado, horário fora da vigência/pausa, conflito, bloqueio, folga ou férias. Consulte `docs/TEAM_WORKFLOW.md`.

## Integração financeira

O fechamento à vista permanece registrado em pagamentos/movimentos de caixa. Vendas a prazo são representadas em `accounts_receivable`; seu recebimento guarda meio e referência. Compras recebidas podem originar `accounts_payable` vinculada ao fornecedor/categoria selecionados. O módulo Financeiro reconcilia esses efeitos sem alterar marcadores do PDV. Consulte [FINANCE_WORKFLOW.md](FINANCE_WORKFLOW.md).

## Alertas e insumos de estoque
O dashboard de Estoque fornece alertas reais de produto/insumo crítico, compras abertas, inventários e transferências pendentes. No pagamento, a ficha técnica é baixada atomicamente e saldo insuficiente reverte a operação com erro rastreável; produtos explícitos não sofrem baixa dupla.

## Integração com BI

Os indicadores gerenciais deste domínio são agregados pelo módulo descrito em `docs/ANALYTICS_WORKFLOW.md`, sem duplicar a fonte operacional canônica.
# Eventos de comunicação

Eventos operacionais publicam chaves idempotentes em `communication_events`. Uma falha de notificação é registrada como `Failed`/`Skipped` e nunca desfaz a transação operacional concluída.

## Sugestões da IA Operacional

A Operação do Dia consulta a contagem de `PendingReview` e encaminha à fila. Aprovar/corrigir seleciona uma comanda e serviço reais, adicionando somente o item; rejeição/correção exigem motivo. Pagamento, comissão e estoque permanecem no fluxo canônico do PDV.

## Governança SaaS
O acesso ao módulo e seus limites são definidos pela assinatura e por `tenant_module_settings`; módulo desabilitado deve falhar claramente, sem fallback. Consulte [GOVERNANCE_WORKFLOW.md](GOVERNANCE_WORKFLOW.md).

## Integração Clube & Vendas
Consulte `CLUB_AND_SALES_WORKFLOW.md` para contratos de assinatura, carteira, resgate, venda pendente, auditoria e regras de origem.

## Integração — Portal do Cliente (Sprint 51)

O fluxo client-scoped, seus limites de privacidade, eventos e comportamento sem provider estão documentados em [CLIENT_PORTAL_WORKFLOW.md](CLIENT_PORTAL_WORKFLOW.md). A integração não aceita identificadores técnicos digitados e não transforma intenção de pagamento em liquidação.

## Integração Qualidade & Retenção — Sprint 52

O contrato de integração, escopo, eventos e restrições está em [QUALITY_AND_RETENTION_WORKFLOW.md](QUALITY_AND_RETENTION_WORKFLOW.md). Os dados permanecem tenant/branch scoped; indisponibilidade não produz resultado fictício, e nenhuma integração usa biometria ou inferência de emoção.

## Integração com Marketplace & Parceiros

A atribuição comercial usa referências rastreáveis e escopo tenant/unidade. Eventos pendentes ou cancelados não confirmam comissão/payout; detalhes e contratos estão em `docs/PARTNERS_MARKETPLACE_WORKFLOW.md`.

## Integração Sprint 57 — Catálogo & Precificação

A operação consome a fonte central de preço, custo, margem, duração, visibilidade e breakdown descrita em [CATALOG_PRICING_WORKFLOW.md](CATALOG_PRICING_WORKFLOW.md). Benefícios e comissões permanecem pendentes até o evento comercial real; escopo de tenant/unidade e trilha de auditoria são obrigatórios.

## Sprint 58 · Atendimento 360

O contrato integrado, estados, transações e responsabilidades deste módulo estão documentados em [SERVICE_EXECUTION_CHECKOUT_WORKFLOW.md](SERVICE_EXECUTION_CHECKOUT_WORKFLOW.md). Eventos reais são correlacionados por `service_order_id`; preview não altera ledger e estados pendentes não são tratados como receita, consumo ou comissão paga.
