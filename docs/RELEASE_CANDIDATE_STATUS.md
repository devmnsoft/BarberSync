# BarberSync 2.0 — status do release candidate

Atualizado em 19 de agosto de 2026. Este documento registra somente verificações
executadas nesta revisão; ausência de ferramenta ou infraestrutura não é tratada
como aprovação.

## Sprint de Produção 5 — estoque, compras, comissão e notificações

A revisão estática encontrou uma quebra operacional concreta: o código de estoque
gravava apenas `payload`, embora `stock_movements` exija produto, tipo, quantidade
e saldo relacionais. Entradas, saídas, ajustes e baixas do PDV agora atualizam
`products.current_stock` e inserem o histórico relacional na mesma transação, com
origem, usuário, tenant e unidade. Ajustes exigem motivo; produto inativo, de outra
unidade ou sem saldo é rejeitado. O dashboard e a consulta de estoque crítico usam
as colunas canônicas.

Recebimentos de compras passaram a atribuir o usuário à movimentação. Índices
idempotentes impedem repetir comissão por item/pagamento, financeiro por
recebimento, movimento de estoque por produto/recebimento e notificação ativa por
entidade/mensagem. Pagamentos de comandas geram comissão somente para itens de
serviço relacionais e com percentual configurado; produtos não entram no cálculo.
Erros do dashboard agora devolvem `traceId`.

Os gates obrigatórios foram tentados antes das alterações. `dotnet` e `psql`
continuam ausentes (código 127), logo build, aplicação/reaplicação SQL, smoke HTTP,
isolamento multi-tenant e UX visual permanecem **não aprovados**. Os checks de
JavaScript, Mobile e Totem passaram. A varredura literal atual encontrou **353
linhas em 114 arquivos**; é uma contagem bruta que inclui documentação e testes,
não uma declaração de 353 fallbacks operacionais. A decisão permanece **NO-GO**.

## Sprint de Produção 4 — revisão de caixa

A revisão encontrou uma divergência concreta entre o nome exigido pelo contrato de
produção (`cash_movements`) e o razão usado pela aplicação (`cash_transactions`).
O schema 016 cria o razão canônico, migra o histórico de forma idempotente e passa
a registrar origem e usuário. Caixa manual, recebimento no PDV e estorno agora
usam a mesma tabela; consultas de saldo, conferência e histórico também leem essa
fonte única. A tabela antiga foi preservada apenas como fonte de migração para não
perder histórico em upgrades.

Os gates .NET e PostgreSQL foram tentados antes das alterações e continuam
bloqueados pela ausência de `dotnet` e `psql` neste executor. Por isso esta rodada
não declara SQL, compilação, fluxos ponta a ponta, módulos restantes ou UX como
aprovados. Mobile, Totem, bundle do Totem e sintaxe JavaScript foram reexecutados
e aprovados. A varredura literal permanece em **351 linhas / 114 arquivos**; esta
correção não introduziu fallback operacional. A decisão permanece **NO-GO**.

## Gates executados

| Gate | Resultado | Evidência / observação |
| --- | --- | --- |
| Mobile | Aprovado | `npm test --prefix MobileApp`: smoke test aprovado. |
| Totem | Aprovado | `npm test --prefix Totem` e `npm run build --prefix Totem`: smoke e bundle Vite aprovados. |
| JavaScript | Aprovado | `node --check` executado em todos os arquivos JavaScript rastreados, excluindo dependências e artefatos. |
| .NET Debug/Release | Bloqueado pelo ambiente | `dotnet --info` falhou porque o SDK .NET não está instalado. Clean, restore e builds não podem ser aprovados nesta máquina. |
| PostgreSQL | Bloqueado pelo ambiente | `psql --version` falhou porque o cliente PostgreSQL não está instalado; o script não foi aplicado e não está aprovado. |
| Smoke HTTP/comercial | Bloqueado | A API não pode ser iniciada sem o SDK e não há PostgreSQL local; nenhum fluxo manual foi declarado como aprovado. |

### Evidência da execução de 19 de agosto de 2026

- `dotnet --info` e `psql --version` retornaram código 127 (`command not found`).
- Como os executáveis necessários não existem no ambiente, não foi possível
  executar honestamente clean, restore, builds, startup ou aplicação do SQL nesta
  revisão. Nenhum desses gates foi inferido a partir de inspeção estática.
- `npm test --prefix MobileApp`, `npm test --prefix Totem` e
  `npm run build --prefix Totem` foram executados novamente e aprovados.
- `node --check` foi executado sobre todos os `.js` de `Web`, `MobileApp` e
  `Totem`, excluindo `node_modules` e `dist`, e não encontrou erro de sintaxe.

## Correções desta revisão

- Cancelamento e no-show agora exigem justificativa no contrato da API e na
  Agenda; a transição continua persistida no histórico com usuário e unidade.
- A remoção de item da comanda agora exige motivo, registra os metadados no item
  excluído e cria evento de auditoria dentro da mesma transação.
- A tela legada de Pagamentos, que mantinha pagamentos fictícios no
  `localStorage` e simulava estorno/recibo, foi removida. A rota antiga redireciona
  ao PDV transacional e o item duplicado saiu da navegação.

- O agendamento Mobile deixou de escolher silenciosamente o primeiro profissional
  e o primeiro horário do resumo. O cliente agora escolhe serviço, profissional e
  data, consulta `/api/mobile/appointments/availability`, seleciona explicitamente
  um horário retornado e somente então confirma a criação.
- A retomada do Totem agora restaura também a etapa persistida pela API. Antes, os
  dados do fluxo eram recuperados, mas a interface sempre reabria na primeira
  etapa, deixando estado e tela inconsistentes.
- Os smokes de Mobile e Totem passaram a rejeitar regressões nesses contratos
  (seleção implícita, falta de consulta real, falta de limpeza ou retomada da sessão).

- Os controladores `DemoOperationsController` e `CommercialOpsController` foram
  removidos. Eles mantinham produtos, comandas, pagamentos, estoque, clientes e
  auditoria em memória e fabricavam aprovações de pagamento. Esses endpoints não
  têm mais como responder com sucesso simulado; os fluxos publicados devem usar os
  controladores transacionais e o PostgreSQL.
- Nove superfícies que ainda dependem de `DemoStore`, armazenamento do navegador ou
  respostas simuladas (Lead to Cash, SaaS Control Center, Full Service Flow, fluxo
  comercial, configurações de plataforma, add-ons, automações, integrações e base
  de conhecimento) agora só respondem em `Development` para `SuperAdmin`.
- O link de integrações simuladas foi removido da navegação operacional, evitando
  um menu morto em produção.
- O controller de estoque agora declara autenticação e as permissões existentes
  `Stock.View`, `Stock.Entry` e `Stock.Adjust` em cada leitura e mutação.
- As mutações CRUD genéricas dos módulos comerciais agora exigem um dos papéis
  `SuperAdmin`, `Owner`, `Admin` ou `Manager`. Pacotes, assinaturas, fornecedores,
  compras e financeiro também declaram autenticação no próprio controller, sem
  depender apenas da política fallback global. As ações transacionais preservam
  suas matrizes específicas para caixa, recepção e profissional.

## Contratos e isolamento revisados estaticamente

- A API mantém autorização autenticada como política fallback; somente entradas
  públicas precisam declarar exceção explicitamente.
- Os controladores de demonstração não registram mais rotas operacionais concorrentes.
- As tabelas recentes estão no script idempotente com nomes canônicos
  `service_recognition_events` e `service_recognition_suggestions` (em vez das
  abreviações `recognition_events` e `recognition_suggestions`).
- Esta revisão estática não substitui os smokes autenticados de perfil, tenant e
  unidade contra PostgreSQL real.

## Varredura de demo e fallback

A varredura literal solicitada, excluindo dependências e artefatos (`node_modules`,
`dist`, `bin` e `obj`), caiu de **393 linhas em 131 arquivos** para **347 linhas em
114 arquivos**. Nesta rodada foram eliminados **2 controladores fake completos**
(345 linhas de código em memória, incluindo 8 ocorrências literais da expressão de
busca), isoladas **9 telas de desenvolvimento** e removido **1 item de menu**.
Ainda existem **347 correspondências brutas** a classificar/corrigir; esse número
inclui documentação, testes browser-side legados, lockfiles e usos técnicos de
`fallback`, e portanto não deve ser apresentado como quantidade de defeitos. A
contagem anterior de 207 pendências operacionais precisa ser reclassificada após
esta remoção, em vez de ser reduzida por estimativa.

Na medição de 19 de agosto, executada com a expressão literal solicitada e
exclusão de `node_modules`, `dist`, `bin` e `obj`, foram encontradas **351 linhas
em 114 arquivos**. O artefato operacional `admin-payments.js`, que continha
`localStorage`, `mock` e sucesso fabricado, foi integralmente removido. As demais
351 correspondências brutas ainda precisam de classificação; incluem este
registro de evidência, documentação, lockfiles e usos técnicos, portanto não são
declaradas como 351 defeitos. O total bruto cresceu em relação à medição anterior
porque a própria documentação de evidência cita os termos pesquisados; a
pendência operacional removida é contabilizada por artefato, não por subtração
artificial dessa métrica autorreferente.

## Pendências reais e riscos

1. Instalar o SDK .NET 10 e executar clean, restore, build Debug, build Release e a suíte da solução.
2. Instalar/iniciar PostgreSQL, criar o banco descartável `barber` e aplicar
   `ScriptsSQL/script_completo.sql` com `ON_ERROR_STOP=1`.
3. Iniciar API, Admin e Kiosk contra esse banco e executar os fluxos comerciais,
   Caixa, pacote/assinatura, compras, IA, Totem e Mobile com evidência de auditoria,
   financeiro, estoque e notificações.
4. Repetir a matriz de papéis e isolamento com dois tenants e duas unidades; a
   inspeção estática isoladamente não prova ausência de vazamento.
5. Executar a matriz visual em navegador real nas larguras 1920, 1440, 1366, 1024,
   768 e 390 px. Nenhuma aprovação visual foi inferida do build do bundle.
6. O repositório ainda contém telas legacy de demonstração e armazenamento local;
   as nove superfícies isoladas nesta rodada não encerram essa limpeza.

## Checklist de publicação

O checklist operacional detalhado, incluindo segurança, backup e rollback, está
em [`PRODUCTION_READINESS_CHECKLIST.md`](PRODUCTION_READINESS_CHECKLIST.md).

- [ ] Build Debug e Release aprovados com SDK .NET 10.
- [ ] Script SQL aplicado duas vezes, sem erro, em banco limpo.
- [x] Sintaxe JavaScript rastreada aprovada.
- [x] Teste do Mobile aprovado.
- [x] Teste e build do Totem aprovados.
- [ ] Smoke ponta a ponta autenticado aprovado.
- [ ] Matriz de permissões, tenant e unidade aprovada.
- [ ] Matriz responsiva com evidência visual aprovada.
- [ ] Varredura legacy/demo concluída para os artefatos publicados.
- [ ] Backup, segredos, CORS, observabilidade e rollback validados no ambiente alvo.

**Decisão atual: NO-GO.** Os gates de compilação .NET, execução SQL e smoke real
continuam obrigatórios e bloqueiam a publicação.
