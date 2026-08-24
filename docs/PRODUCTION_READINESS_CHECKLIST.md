# BarberSync 2.0 — checklist de prontidão para produção

Atualizado em 20 de agosto de 2026. Este checklist é um gate de publicação: um
item sem evidência permanece reprovado, mesmo quando a implementação existe no
repositório.

## Gate local da Sprint de Produção 16

- [x] Docker Compose versionado com PostgreSQL 16, SDK .NET 10, Node 20, API na
  porta 5080 e banco efêmero `barber`.
- [x] Scripts Bash e PowerShell executam o mesmo fluxo, falham no primeiro erro,
  preservam logs e sempre removem os containers.
- [x] O fluxo contém restore, builds Debug/Release, duas aplicações SQL,
  validação de tabelas críticas, API/health/smoke e todos os checks frontend.
- [x] GitHub Actions invoca o script versionado, sem duplicar rotas ou comandos.
- [x] O gate não depende de GitHub CLI, token, API do GitHub, `dotnet` ou `psql`
  instalados no host; `BarberSync.Tests` e `dotnet test` permanecem fora.
- [ ] Execução Docker completa neste executor: bloqueada porque o comando
  `docker` não está instalado. Nenhum gate runtime foi aprovado por inferência.
- [x] Checks disponíveis no host passaram: `node --check`, Mobile smoke, Totem
  smoke e build Totem.
- [ ] Scan demo/fallback encerrado: a busca solicitada ainda encontra 48 arquivos
  que incluem legado isolado, testes e usos técnicos a classificar.

## Auditoria contratual da Sprint de Produção 15

- [x] Todas as rotas chamadas pelo smoke foram confrontadas com actions reais; o
  dashboard usa `/api/dashboard/summary`, financeiro usa `/api/finance` e caixa
  usa a action existente `/api/cash-registers/current`.
- [x] O próprio script aguarda `/health` com tentativas limitadas e continua
  exigindo status exato, sem aceitar 200, 404 ou 500 em rota protegida.
- [x] A matriz 401 cobre dashboard, notificações, financeiro, estoque, caixa,
  comandas, compras, reconhecimento de serviço e configurações de IA, exigindo
  também correlação em cada resposta.
- [x] Política fallback, `Authorize`, roles e permissões dos controllers
  operacionais foram revisados estaticamente; nenhuma abertura pública foi
  adicionada. `/health` é anônimo por desenho para readiness.
- [x] Health usa `IBarberSchemaInitializer`, responde 200/503 segundo banco e
  schema e publica somente estado sanitizado; observabilidade adiciona
  `X-Trace-Id` e o handler global produz JSON correlacionado sem stack trace.
- [x] Workflow mantém PostgreSQL 16, SDK .NET 10 para `net10.0`, SQL duas vezes,
  builds Debug/Release, checks frontend e porta 5080, sem `continue-on-error`.
- [x] Revisão estática do SQL confirmou guardas nas tabelas/índices/colunas,
  migração do caixa com `ON CONFLICT` e versão do schema sem chave duplicada.
- [ ] Execução local de .NET, PostgreSQL e smoke HTTP: `dotnet` e `psql` não estão
  instalados; nenhuma aprovação runtime foi inferida da auditoria estática.
- [x] Checks locais disponíveis passaram; scan focado registrou 188 linhas em 48
  arquivos e não encontrou ocorrência operacional nova nos arquivos alterados.
- [x] `BarberSync.Tests` permanece intocado e `dotnet test` continua reservado
  para a fase final.

## Gate obrigatório da Sprint de Produção 11

- [x] `.github/workflows/production-readiness.yml` criado para pull requests de
  `main` e execução manual, com PostgreSQL 16 saudável, .NET SDK 10 e Node.js 20.
- [x] Job configurado para executar `dotnet --info`, `psql --version`, restore e
  builds Debug/Release reais, sem `dotnet test` e sem incluir `BarberSync.Tests`
  na solução.
- [x] Job configurado para aplicar `ScriptsSQL/script_completo.sql` com
  `ON_ERROR_STOP=1`, listar o schema e falhar se qualquer uma das dez tabelas
  críticas estiver ausente.
- [x] Job configurado para subir a API Release contra o PostgreSQL do service,
  aguardar `/health` e executar `scripts/production-smoke.sh`.
- [x] Smoke operacional cobre health/banco, login inválido tratado, autenticação
  dos endpoints protegidos e presença de correlação nas respostas de erro.
- [x] `node --check`, smoke Mobile, smoke Totem e build Totem estão no mesmo gate;
  os quatro checks também passaram localmente em 20 de agosto de 2026.
- [ ] Primeira execução hospedada completamente verde. Até essa evidência existir,
  restore, builds, SQL, runtime e smoke HTTP permanecem **NO-GO**, pois este
  executor local não fornece `dotnet` nem `psql`.
- [ ] Classificação literal encerrada: a medição atual é 188 linhas/48 arquivos,
  sem alteração nesta sprint; permanecem legado, superfícies dev-only protegidas
  e usos técnicos a revisar, sem novo fallback operacional identificado.
- [x] `BarberSync.Tests` continua fora da solução e nenhuma classe de teste foi
  tocada; `dotnet test` continua reservado para a fase final.

## Critérios de entrada do release candidate

- [ ] `dotnet clean BarberSync.sln` aprovado.
- [ ] `dotnet restore BarberSync.sln` aprovado.
- [ ] Builds Debug e Release aprovados, sem desabilitar módulos ou regras.
- [ ] `ScriptsSQL/script_completo.sql` aplicado com `ON_ERROR_STOP=1` em banco
  PostgreSQL limpo e reaplicado para comprovar idempotência.
- [x] Sintaxe dos JavaScripts publicados validada com `node --check`.
- [x] Smoke do Mobile aprovado.
- [x] Smoke e bundle de produção do Totem aprovados.
- [ ] API, AdminWeb e KioskWeb iniciados contra PostgreSQL real.
- [ ] Varredura de resíduos demo concluída, sem ocorrência operacional pendente.
- [ ] Nenhuma classe de `BarberSync.Tests` alterada nesta fase.

### Evidência da Sprint de Produção 9

- [ ] SDK .NET: `dotnet --info` não pôde ser executado porque o binário não existe
  no executor; uma busca no sistema também não encontrou instalação alternativa.
- [ ] PostgreSQL: `psql --version` não pôde ser executado porque o cliente não
  existe. A tentativa de instalar PostgreSQL via APT foi recusada pelo proxy com
  HTTP 403, portanto o schema não foi aplicado.
- [ ] API, AdminWeb, KioskWeb, SystemHealth e smokes autenticados: não executados,
  pois dependem dos dois gates anteriores. Nenhum resultado foi inferido.
- [x] JavaScript publicado passou em `node --check`; Mobile e Totem passaram nos
  smokes contratuais e o bundle de produção do Totem foi gerado.
- [ ] Limpeza literal concluída: a medição encontrou 377 linhas/123 arquivos no
  repositório e 206 linhas/57 arquivos no recorte das aplicações, ainda pendentes
  de classificação integral.
- [x] `BarberSync.Tests` permaneceu fora desta fase e `dotnet test` não foi
  executado, conforme o plano de release.

### Evidência parcial desta sprint

- [x] Em 19 de agosto de 2026, todos os 101 arquivos `.js` rastreados passaram
  em `node --check`; Mobile smoke, Totem smoke e bundle Vite do Totem passaram.
- [ ] Clean, restore, builds Debug/Release, SQL e startups da Sprint 7: o executor
  não possui `dotnet` nem `psql` (ambos retornaram código 127).
- [ ] Smokes Presencial, Tenant/Branch, Totem E2E, Mobile Cliente E2E, Mobile
  Profissional E2E, IA, Relatórios, Notificações e Auditoria: dependem da API e
  do PostgreSQL reais e não foram declarados aprovados.
- [ ] Limpeza literal encerrada: a medição atual encontrou 393 linhas/129
  arquivos no repositório e 150 linhas/36 arquivos no recorte de código das
  aplicações; ocorrências isoladas de desenvolvimento e usos técnicos ainda
  precisam ser separados de resíduos operacionais.

- [x] Filtros de período do relatório executivo chegam ao backend e são usados
  também pela exportação CSV; intervalo incompleto ou invertido é rejeitado.
- [x] Exportação executiva registra usuário autenticado, tenant, unidade,
  correlação e período em `audit_logs`.
- [x] Estoque crítico executivo consulta as colunas relacionais canônicas.
- [x] Leitura individual e coletiva de notificações persiste `read_at`/`status`
  e é limitada ao tenant e unidade das claims.
- [x] Lista de notificações expõe estado de leitura e vínculo da entidade a partir
  das colunas relacionais, sem depender de payload como banco operacional.
- [ ] Relatórios obrigatórios restantes e equivalência completa dos dashboards
  executados contra banco real; a ausência de .NET/PostgreSQL impede aprovação.

- [x] Estoque manual e baixa no PDV usam saldo e histórico relacionais em uma transação, com tenant, unidade, origem e usuário.
- [x] Ajuste manual rejeita motivo ausente; produto inativo, fora da unidade ou sem saldo é rejeitado.
- [x] Recebimento de compra mantém custo médio e registra usuário; índices protegem estoque e financeiro contra repetição do mesmo recebimento.
- [x] Comissão de pagamento é restrita a item de serviço e idempotente por pagamento/item.
- [x] Notificação ativa é idempotente por unidade, entidade e mensagem e possui link acionável.
- [x] Falha do dashboard inclui `traceId`.
- [ ] Fluxos acima executados contra PostgreSQL real; `psql` não existe no executor.
- [ ] Builds Debug/Release; `dotnet` não existe no executor.
- [ ] UX de Estoque, Compras, Financeiro, Caixa, Relatórios, Dashboard, Auditoria, Notificações e PDV validada em navegador real.

- [x] Caixa manual, pagamento em dinheiro no PDV e estorno escrevem no razão canônico `cash_movements`, com origem, usuário, tenant e unidade.
- [x] Migração 016 copia idempotentemente o histórico de `cash_transactions`; saldo, conferência e histórico consultam a mesma fonte.
- [ ] Aplicação real da migração 016 pendente: `psql` não está instalado neste executor.

- [x] Mobile exige escolha explícita de profissional e horário retornado pela disponibilidade real antes de criar o agendamento.
- [x] Totem retoma a etapa mantida no servidor e conserva a limpeza remota ao concluir o fluxo.
- [x] Smokes estáticos cobrem os contratos de disponibilidade Mobile e retomada/limpeza do Totem.
- [x] Cancelamento, no-show e remoção de item exigem justificativa; a remoção de
  item é auditada na mesma transação.
- [x] Pagamentos legados em `localStorage` e seus sucessos simulados foram
  removidos; `/Admin/Payments` conduz ao PDV real.

- [x] Controladores que simulavam pagamento, estoque, comanda e auditoria em memória removidos.
- [x] Telas ainda dependentes de armazenamento local isoladas em `Development` + `SuperAdmin`.
- [x] Estoque protegido explicitamente por autenticação e `Stock.View`, `Stock.Entry` ou `Stock.Adjust`.
- [ ] SDK .NET disponível no executor (`dotnet --info`: comando ausente).
- [ ] PostgreSQL disponível no executor (`psql --version`: comando ausente).
- [ ] As 351 correspondências brutas restantes da varredura foram classificadas e resolvidas.

## Smokes funcionais obrigatórios

Cada cenário deve registrar os identificadores das entidades, usuário, tenant,
unidade, horário UTC e `traceId` de eventual erro.

- [ ] **Presencial:** cliente → agenda → check-in → atendimento → comanda → PDV
  → caixa → financeiro → estoque → comissão → relatório.
- [ ] **Totem:** unidade → identificação mínima → disponibilidade → check-in →
  pré-comanda → Atendimento/PDV → limpeza da sessão.
- [ ] **Mobile Cliente:** login real → agenda/reagenda/cancela → histórico,
  pacotes, assinatura, cashback, cupons e notificações próprios.
- [ ] **Mobile Profissional:** agenda própria → início/fim do atendimento →
  bloqueio com motivo → comissão somente leitura.
- [ ] **IA:** sugestão → confirmação humana → item de comanda → auditoria; rejeição
  sem alteração da comanda; indisponibilidade do provider sem afetar a operação.
- [ ] Estorno reverte os efeitos aplicáveis de financeiro, estoque e comissão.
- [ ] Falhas funcionais exibem mensagem segura e `traceId`, nunca sucesso simulado.

## Segurança, isolamento e LGPD

- [ ] Matriz Owner, SuperAdmin, Admin, Manager, Receptionist, Cashier,
  Professional, Client e Totem/Public validada em menu, botão, endpoint e query.
- [ ] Cenários executados com dois tenants e duas unidades, comprovando ausência
  de vazamento em dashboards, relatórios, auditoria e notificações.
- [ ] Cliente e profissional somente acessam os próprios recursos permitidos.
- [ ] CORS usa allowlist; cookies são `Secure`, `HttpOnly` e têm `SameSite`
  apropriado; tokens e segredos vêm do secret store do ambiente.
- [ ] Produção não expõe stack trace, Swagger/diagnóstico público ou segredos.
- [ ] Reconhecimento de serviço exige configuração/consentimento; imagem ou
  biometria não é persistida por padrão; IA nunca cobra automaticamente.
- [ ] Usuário inicial é provisionado por canal seguro, sem senha padrão no código.

## Observabilidade e operação

- [ ] Health checks de API e banco monitorados sem expor detalhes sensíveis.
- [ ] Logs estruturados correlacionam `traceId`, tenant e unidade, sem PII ou token.
- [ ] Alertas de erro, latência, estoque crítico e fila de IA configurados.
- [ ] Connection strings de produção e política de rotação validadas.
- [ ] Retenção de auditoria, notificações e dados pessoais aprovada.

## UX e responsividade

- [ ] Login, Dashboard, PDV, Caixa, Agenda, Atendimento, Cliente 360, Equipe,
  Estoque, Compras, Financeiro, Relatórios, Auditoria, Notificações,
  ServiceRecognition, AiSettings e SystemHealth revisados.
- [ ] Totem revisado em orientação horizontal e vertical; Mobile Cliente e
  Profissional revisados em dispositivo real.
- [ ] Matriz visual aprovada em 1920×1080, 1440×900, 1366×768, 1024×768,
  768 px e 390 px, incluindo teclado, foco, contraste, loading, empty e erro.
- [ ] Não há botão principal sem ação, rota inexistente ou overflow impeditivo.

## Backup, publicação e rollback

- [ ] Backup completo criado e restauração ensaiada em ambiente descartável.
- [ ] Migração possui estimativa de duração, impacto de lock e responsável.
- [ ] Imagens/artefatos são imutáveis e identificados pelo commit do release.
- [ ] Janela, responsáveis, comunicação e critérios objetivos de abortar definidos.
- [ ] Rollback da aplicação mantém compatibilidade com o schema implantado.
- [ ] Rollback de dados usa restauração/PITR aprovada; migrations destrutivas não
  são revertidas de forma improvisada.
- [ ] Smoke pós-publicação e monitoramento intensivo têm responsável e duração.

## Decisão

O release só pode mudar para **GO** quando todos os itens obrigatórios acima
tiverem evidência anexada ao registro do candidato. O estado atual é **NO-GO**.

## Sprint de Produção 18 — limpeza dos 15 assets operacionais

- [x] Os 15 assets pendentes foram revisados: 11 bundles legados, não carregados
  por nenhuma view, foram removidos; 4 laboratórios permanecem protegidos por
  `Development` + `SuperAdmin`.
- [x] O serviço de dados não inventa tenant/unidade para chamadas anônimas e
  rejeita configuração ausente, UUID vazio ou inválido com mensagem explícita.
- [x] `.env.example` e o compose do gate declaram ambos os escopos obrigatórios.
- [ ] Gate Docker, builds .NET, SQL idempotente, smokes autenticados e matriz
  tenant/unidade executados. Até essa evidência, a decisão permanece **NO-GO**.
- [x] Endpoint legado de configuração pública estática removido; Copilot exige
  tenant explícito e devolve somente sugestões do serviço real, inclusive vazio.

## Sprint de Produção 22 — execução externa assistida

- [x] Pré-checagens Bash e PowerShell verificam ferramentas, autenticação e variáveis sem exibir segredos, avaliam as rotas Docker, host e GitHub Actions e não param no primeiro requisito ausente.
- [x] Coletores Bash e PowerShell registram Git/PR, ambiente, gate, logs, checks frontend e scan em `artifacts/release-evidence/`, que permanece ignorado pelo Git.
- [x] O gate grava marcadores de sucesso somente depois de cada comando crítico concluir com código zero; os parsers exigem todos os marcadores e o fechamento do PR #201.
- [x] Runbook de execução externa e template de evidência foram versionados.
- [x] Production smoke continua exigindo `/health` 200, status 401 exato nas onze rotas protegidas, login inválido 400/401 e correlação nos erros aplicáveis.
- [ ] PR #201 fechado: pendência manual, pois este executor não possui GitHub CLI autenticado.
- [ ] Gate real completo: **NO-GO** até a coleta de evidência em máquina Docker/.NET/PostgreSQL ou em GitHub Actions autenticado.
- [x] Checks disponíveis da Sprint 22 foram executados: sintaxe Bash/JavaScript, Mobile smoke, Totem smoke, Totem build e integridade do diff.
- [x] `BarberSync.Tests` permanece fora e `dotnet test` continua reservado para a fase final.

## Authenticated readiness additions

- [ ] Isolated seed ran with `EVIDENCE:READINESS_SEED:PASS`.
- [ ] All four real JWT logins and ownership assertions passed.
- [ ] Dashboard, notifications, stock, and current cash register returned scoped persisted data.
- [ ] Explicit kiosk device succeeded and missing deviceCode failed clearly.
- [ ] `authenticated-production-smoke.log` contains every required PASS marker.
- [ ] POS evidence contains exact PASS markers for service order, payment, stock movement, cash movement, financial entry, commission and aggregate POS; absent or skipped markers fail the gate.
- [ ] POS is not SKIPPED before declaring GO.

## Auditoria estática pré-gate

- [ ] `validate-readiness-contracts` passa nas versões Bash e PowerShell disponíveis.
- [ ] `READINESS_CONTRACTS_STATIC:PASS` consta no log do runner.
- [ ] Contratos `PaymentId`/`payment_id`, POS, estoque, seed protegido e markers foram verificados.
- [ ] Consultas filhas de caixa e listagens enterprise mantêm escopo explícito de tenant/unidade, e os índices de idempotência do financeiro e das comissões foram verificados.
- [ ] Build .NET, schema PostgreSQL e smokes reais continuam obrigatórios; sem eles a decisão permanece **NO-GO**.

## Sprint de Produção 29 — evidência oficial no GitHub Actions

- [x] `Production Readiness` executa em pull requests e pushes para `main`, além de execução manual, com concorrência, timeout e permissões mínimas.
- [x] Logs do gate e evidências de release são publicados por `if: always()` e o Step Summary mostra markers passados, ausentes e falhos.
- [x] Ausência de log, artifact ou marker PASS resulta em **NO-GO**; o summarizer mantém código de saída não zero mesmo que etapas anteriores falhem.
- [ ] Todos os markers foram produzidos pelo gate Docker real no GitHub Actions. Até essa execução terminar com PASS integral, o estado permanece **NO-GO**.
