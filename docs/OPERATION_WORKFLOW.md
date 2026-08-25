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
