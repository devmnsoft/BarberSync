# BarberSync 2.0 — status do release candidate

Atualizado em 19 de agosto de 2026. Este documento registra somente verificações
executadas nesta revisão; ausência de ferramenta ou infraestrutura não é tratada
como aprovação.

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
