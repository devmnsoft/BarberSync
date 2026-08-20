# BarberSync 2.0 — status do release candidate

Atualizado em 20 de agosto de 2026. Este documento registra somente verificações
executadas nesta revisão; ausência de ferramenta ou infraestrutura não é tratada
como aprovação.

## Sprint de Produção 16 — gate local versionado

O gate do CI agora é um único contrato versionado, executável por
`scripts/run-production-readiness.sh` e pelo equivalente PowerShell. O Docker
Compose fixa PostgreSQL 16, SDK .NET 10, Node 20, porta 5080 e as credenciais
efêmeras do banco `barber`. O fluxo preserva restore, builds Debug/Release, duas
aplicações do SQL com `ON_ERROR_STOP`, validação das tabelas críticas, API,
`/health`, production smoke e os quatro checks frontend. Todos os logs ficam em
`artifacts/production-readiness` e os containers são removidos ao terminar.

O workflow passou a invocar esse mesmo gate, eliminando cópias divergentes dos
comandos. Ele não usa `gh`, `GH_TOKEN` ou `api.github.com`; não executa
`dotnet test` e não altera `BarberSync.Tests`.

Neste executor, `docker` não está instalado. A execução Docker foi iniciada e
interrompida corretamente pela validação de pré-requisito, portanto PostgreSQL,
restore/build, reaplicação SQL, API, health e production smoke **não são
declarados aprovados**. Os checks executáveis no host — sintaxe JavaScript,
Mobile smoke, Totem smoke e build Totem — passaram. O scan literal continua em
48 arquivos e sua classificação integral permanece pendente. A decisão segue
**NO-GO** até uma execução completa do script em um host Docker.

## Sprint de Produção 15 — contrato do production smoke

A auditoria estática confirmou os mapeamentos reais usados pelo smoke: `/health`,
`/api/dashboard/summary`, `/api/notifications`, `/api/finance`, `/api/stock`,
`/api/cash-registers/current`, `/api/service-orders`, `/api/purchases`,
`/api/service-recognition/suggestions`, `/api/system/ai-settings` e
`/api/auth/login`. Não existe action `GET /api/reports` nem `GET
/api/cash-registers`; por isso o gate usa, respectivamente, o módulo financeiro
real e a action `current`, sem transformar 404/405 em sucesso.

O smoke agora possui sua própria espera limitada por `/health` e ampliou a matriz
401 para compras, reconhecimento de serviço e configurações de IA. Cada rota
protegida continua exigindo exatamente 401 e correlação por `traceId` ou
`X-Trace-Id`; login inválido aceita somente os contratos 400/401. A política
fallback autenticada protege dashboard e notificações, e os demais controllers
auditados também declaram `Authorize`/roles/permissões. `/health` permanece a
única rota operacional do smoke explicitamente anônima e retorna 200/503 conforme
o resultado de `IBarberSchemaInitializer`.

O workflow foi conferido sem alteração: .NET 10 coincide com `net10.0`, PostgreSQL
16 possui healthcheck, o SQL idempotente é aplicado duas vezes, builds Debug e
Release e checks frontend permanecem obrigatórios, e API/smoke usam a porta 5080.
O SQL canônico já usa guardas idempotentes nas estruturas recentes, `ON CONFLICT`
na migração do caixa e na versão do schema. `dotnet` e `psql` não existem neste
executor, portanto build, reaplicação real do SQL e smoke HTTP permanecem sem
aprovação local. Os checks disponíveis passaram. O scan focado permaneceu em 188
linhas/48 arquivos; não apontou ocorrência operacional nova nos arquivos tocados
e a classificação integral do legado isolado continua pendente. O estado segue
**NO-GO** até evidência runtime/hospedada.

## Sprint de Produção 11 — gate executável de produção

Foi criado o workflow obrigatório `Production Readiness`, acionado em pull
requests para `main` e manualmente. O job Ubuntu instala o SDK .NET 10 (framework
usado por todos os projetos da solução) e Node.js 20, provisiona PostgreSQL 16 e
executa, em ordem, `dotnet --info`, `psql --version`, restore, builds Debug e
Release, aplicação do `script_completo.sql` com `ON_ERROR_STOP`, validação das dez
tabelas críticas, checks frontend, startup da API em Release, `/health` e smoke
HTTP operacional. A connection string e as configurações efêmeras de CORS/JWT
ficam apenas no ambiente do job.

O smoke `scripts/production-smoke.sh` exige banco saudável, 401 nos endpoints
protegidos de dashboard, notificações, relatórios/financeiro, estoque, caixa e
comandas, erro tratado no login inválido e correlação por `traceId` ou
`X-Trace-Id`. O log da API é publicado como artefato quando o job falha.

Neste executor local, os checks de JavaScript, Mobile, Totem e bundle Totem
passaram. `dotnet` e `psql` continuam indisponíveis localmente; ao contrário das
sprints anteriores, esses gates agora são executados pelo runner hospedado com
as ferramentas e o PostgreSQL real, e uma falha interrompe o workflow. O estado
permanece **NO-GO até a primeira execução verde do workflow no pull request**;
nenhum resultado remoto foi antecipado neste documento.

A busca literal solicitada encontrou **188 linhas em 48 arquivos** antes e depois
desta alteração. Não foi introduzida ocorrência nem encontrado novo fallback em
superfície operacional tocada. As superfícies DemoStore restantes já são legado
ou ferramentas deliberadamente isoladas por `DevelopmentOnly` + `SuperAdmin`, e
os usos de “fallback” do cliente HTTP representam tratamento de falha sem fabricar
sucesso. A classificação integral das superfícies legadas permanece uma pendência
real; nenhuma contagem foi artificialmente reduzida.

## Sprint de Produção 9 — tentativa de validação real

A validação foi iniciada em um executor Ubuntu 24.04 em 19 de agosto de 2026.
`dotnet` e `psql` não estavam presentes. Também foi tentada a instalação real de
PostgreSQL pelo gerenciador do sistema (`apt-get update && apt-get install -y
postgresql postgresql-client wget ca-certificates`), mas o proxy do ambiente
recusou os repositórios Ubuntu com HTTP 403. Não há binários alternativos de
`dotnet` ou `psql` no sistema. Assim, este registro **não** aprova por inferência
clean, restore, builds, SQL, startups, SystemHealth ou qualquer smoke autenticado.
A decisão permanece **NO-GO**.

Os gates disponíveis foram reexecutados: sintaxe JavaScript publicada, smoke
contratual Mobile, smoke contratual Totem e bundle Vite do Totem passaram. A
varredura literal, sem dependências ou artefatos, contabilizou **377 linhas em
123 arquivos** no repositório e **206 linhas em 57 arquivos** em `Backend`,
`Web`, `MobileApp` e `Totem`. As ocorrências incluem superfícies deliberadamente
isoladas por `DevelopmentOnly` + `SuperAdmin`, documentação e usos técnicos; elas
não foram convertidas artificialmente em defeitos nem declaradas resolvidas.
`BarberSync.Tests` permaneceu intocado e `dotnet test` não foi executado.

## Sprint de Produção 7 — gates reais e smokes autenticados

Os gates foram reexecutados no executor do candidato em 19 de agosto de 2026.
`dotnet --info` e `psql --version` falharam com código 127 porque os executáveis
não estão instalados. Por isso não foram executados clean, restore, builds,
aplicação do schema, startup dos três processos nem smokes autenticados. A
decisão continua **NO-GO**; inspeção estática não foi usada para promover
nenhum desses gates.

Os 101 arquivos JavaScript rastreados pelo Git passaram em `node --check`. Os
smokes contratuais de Mobile e Totem passaram, e o bundle Vite de produção do
Totem foi gerado com sucesso (26 módulos transformados). Esses scripts verificam
arquivos e contratos essenciais no código; eles não substituem os fluxos
autenticados contra API e PostgreSQL, que permanecem pendentes.

A varredura literal solicitada, excluindo dependências e artefatos, encontrou
**393 linhas em 129 arquivos**. No recorte de código publicado dos quatro
diretórios de aplicação, excluindo Markdown e lockfile, foram **150 linhas em 36
arquivos**. Há superfícies de demonstração deliberadamente isoladas em
`Development` + `SuperAdmin`, usos técnicos da palavra `fallback` e resíduos que
ainda precisam de classificação; portanto a meta de limpeza não está aprovada.
Nenhuma classe de `BarberSync.Tests` foi alterada e `dotnet test` não foi
reativado nesta fase.

## Sprint de Produção 6 — relatórios, auditoria e notificações

O relatório executivo agora envia o período escolhido à API e a exportação CSV
reutiliza os mesmos parâmetros da tela. O backend valida intervalos, aplica o
período ao faturamento e ticket médio e registra a exportação com `user_id`,
tenant, unidade e intervalo. O KPI de estoque crítico deixou de ler o JSON legado
e usa diretamente `products.current_stock` e `minimum_stock`.

Notificações agora possuem projeção própria das colunas relacionais, inclusive
estado de leitura e entidade. As ações individual e em lote persistem `read_at` e
`status` no banco, sempre filtradas pelo tenant e unidade autenticados; a tela não
faz mais uma atualização genérica de payload nem dispara uma requisição por item.
Todos os novos erros técnicos retornam `traceId`.

Os gates obrigatórios foram tentados antes das alterações. `dotnet` e `psql` não
existem no executor (código 127), portanto compilação, aplicação SQL, smoke HTTP,
matriz tenant/branch e revisão visual continuam sem aprovação. JavaScript, Mobile
e Totem passaram. A varredura literal encontrou **353 linhas em 114 arquivos**,
mesmo total bruto anterior; nenhum fallback operacional foi introduzido ou
removido nesta rodada. A decisão permanece **NO-GO**.

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

## Sprint de Produção 18 — encerramento da classificação operacional

Os 15 assets classificados como operacionais foram inspecionados. Onze bundles
legados não eram carregados por view/layout e foram removidos, eliminando código
de campanha, dashboard, estoque, assinatura e outros fluxos baseado em DemoStore
ou armazenamento do navegador. Quatro laboratórios legítimos continuam somente
em `Development` e para `SuperAdmin`; promovê-los exigirá API e persistência real.

O escopo de PublicWeb/Totem agora depende obrigatoriamente de UUIDs explícitos de
tenant e unidade. Claims autenticadas inválidas continuam rejeitadas e não há
UUID fixo no runtime do serviço. A decisão permanece **NO-GO** até o gate Docker,
builds, banco, smokes autenticados, matriz de isolamento e validação visual.

A busca complementar eliminou ainda a configuração pública estática (branding,
serviços, profissionais e Totem fabricados) e o fallback de sugestões do Copilot.
O Copilot agora rejeita tenant vazio e preserva o estado vazio do serviço real.

## Sprint de Produção 19 — auditoria pós-remoção

A auditoria encontrou um consumidor órfão do novo contrato de escopo: a página
legacy `/Copilot` chamava `/api/copilot/suggestions` sem `tenantId`. O cliente
agora lê exclusivamente a claim assinada `tenant_id` exposta pelo servidor e a
envia explicitamente; se a claim estiver ausente, a tela interrompe a chamada e
exibe erro, sem UUID default ou estado demonstrativo. O endpoint também trata
UUID ausente, malformado, vazio ou zero de modo uniforme com HTTP 400, `traceId`
e `X-Trace-Id`.

Não foram encontradas referências runtime a `PublicConfigController`,
`ConfigurationService`, `/api/public-config` ou aos onze bundles removidos. A
checagem dos `script src` locais também não encontrou assets inexistentes. O
compose e `.env.example` usam os nomes `BarberSync__DefaultTenantId` e
`BarberSync__DefaultBranchId` coerentes com as chaves lidas pela API.

Docker, SDK .NET e `psql` não estão instalados neste executor. O gate foi
acionado e parou imediatamente com `ERROR: Docker is required.`; portanto build
.NET, SQL, API, health e smoke E2E continuam sem evidência. Sintaxe dos scripts,
smokes Mobile/Totem e build Totem foram aprovados. A decisão permanece
**NO-GO**.
