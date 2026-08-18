# BarberSync 2.0 — status do release candidate

Atualizado em 18 de agosto de 2026. Este documento registra somente verificações
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

## Correções desta revisão

- Os controladores legados de demonstração deixaram de disputar `/api/products`,
  `/api/service-orders`, estoque, auditoria, check-in e Full Service Flow com os
  controladores transacionais. Todos os recursos legados agora ficam nos namespaces
  explícitos `/api/demo-commerce` ou `/api/demo-operations`.
- A mudança elimina seleção ambígua de action e impede que uma resposta em memória
  seja servida acidentalmente por uma rota operacional. A política privada global
  continua sendo aplicada aos endpoints legados.
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

O comando solicitado encontrou **350 ocorrências**: **0 removidas nesta revisão**,
**139 classificadas em documentação/legacy**, **0 em testes deferidos** e **211 em
código ou interface ainda pendentes de isolamento/remoção**. Termos técnicos como
`fallback` também entram na contagem textual; portanto, cada ocorrência pendente
precisa de classificação funcional antes da publicação. Nenhuma foi ocultada ou
declarada resolvida apenas por documentação.

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
6. O repositório ainda contém telas legacy de demonstração e armazenamento local.
   Elas precisam ser removidas ou isoladas de navegação e publicação antes do go-live.

## Checklist de publicação

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
