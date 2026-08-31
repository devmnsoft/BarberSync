# Central de Controle

A Central de Controle reúne snapshots executivos, operação do dia, alertas, incidentes, tarefas e verificações persistidas de integração. Todas as consultas são autenticadas, exigem permissão e aplicam o `tenant_id` e `branch_id` obtidos dos claims.

## Fluxos

1. **Executivo:** lê o último snapshot consolidado; ausência resulta em estado vazio, nunca em indicador inventado.
2. **Operação:** consulta agenda, comandas e pendências diretamente no banco da unidade.
3. **Alertas:** `Open` pode ser reconhecido; abertos ou reconhecidos podem ser resolvidos/descartados. Descartar `High`/`Critical` exige motivo.
4. **Incidentes:** `Investigating` exige responsável e resolução exige notas. A auditoria operacional registra as escritas.
5. **Tarefas:** prioridade, responsável opcional e prazo; conclusão grava usuário e instante UTC do banco.
6. **Health:** exibe somente checks persistidos. Ausência/configuração desconhecida permanece `NotConfigured`/`Unknown`.

## Fontes e UX

Agenda, PDV, financeiro, estoque, equipe, Clientes 360, Portal, Clube, Qualidade, Marketing, Parceiros, Comunicação, IA, Governança e readiness são representados por snapshots/checks com `sourceStatus`. A interface oferece cards, matriz, kanban, fila, badges, skeleton, estado vazio e painel de erro com trace ID. IDs internos nunca são digitados: responsáveis vêm de `/filter-options`.

## Segurança e relatórios

As permissões `CommandCenter.*` são seedadas de forma idempotente. O CSV contém somente snapshots do tenant e filial da sessão. Metadados devem evitar dados pessoais desnecessários e seguir a política LGPD existente.

## Integração Sprint 57 — Catálogo & Precificação

A operação consome a fonte central de preço, custo, margem, duração, visibilidade e breakdown descrita em [CATALOG_PRICING_WORKFLOW.md](CATALOG_PRICING_WORKFLOW.md). Benefícios e comissões permanecem pendentes até o evento comercial real; escopo de tenant/unidade e trilha de auditoria são obrigatórios.

## Sprint 58 · Atendimento 360

O contrato integrado, estados, transações e responsabilidades deste módulo estão documentados em [SERVICE_EXECUTION_CHECKOUT_WORKFLOW.md](SERVICE_EXECUTION_CHECKOUT_WORKFLOW.md). Eventos reais são correlacionados por `service_order_id`; preview não altera ledger e estados pendentes não são tratados como receita, consumo ou comissão paga.

## Integração Sprint 60 — Equipe & RH 360

O contrato canônico e as regras de isolamento, disponibilidade, produtividade, comissão, qualidade, alertas e relatórios estão descritos em [TEAM360_WORKFLOW.md](TEAM360_WORKFLOW.md). A integração preserva a origem real e publica `sourceStatus` quando uma fonte não está disponível.
