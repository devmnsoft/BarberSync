# Classificação de demo e fallback — Sprint de Produção 18

Atualizado em 20 de agosto de 2026. A classificação considera apenas fontes
publicadas (dependências e `dist` são excluídos) e não transforma uma ocorrência
literal em aprovação de runtime.

## Resultado dos 15 assets operacionais

Os 15 itens pendentes foram abertos e confrontados com as views e layouts que
publicam JavaScript. Onze eram bundles legados sem qualquer referência em view;
eles ainda continham DemoStore, dados estáticos ou armazenamento operacional e
foram removidos, em vez de preservados como uma segunda implementação. Quatro
são ferramentas de demonstração legítimas e permanecem inacessíveis fora de
`Development` e a usuários que não sejam `SuperAdmin`, pela guarda única de
`AdminController.DevelopmentOnly`.

### Corrigido nesta sprint (11)

| Asset removido | Padrão operacional eliminado | Implementação publicada |
|---|---|---|
| `wwwroot/js/admin-campaigns.js` | campanhas e cupons no DemoStore | `Campaigns.cshtml` usa `AdminApi/campaigns` |
| `wwwroot/js/admin-channel-manager.js` | estado de canais simulado | view atual não carrega o bundle legado |
| `wwwroot/js/admin-client-onboarding.js` | progresso operacional em `localStorage` | view atual não carrega o bundle legado |
| `wwwroot/js/admin-copilot.js` | respostas e ações simuladas | view atual não carrega o bundle legado |
| `wwwroot/js/admin-customer-journey.js` | jornada derivada do DemoStore | view atual não carrega o bundle legado |
| `wwwroot/js/admin-dashboard.js` | KPIs e sucesso por fallback | `Dashboard.cshtml` usa `admin-dashboard-premium.js` e API real |
| `wwwroot/js/admin-help.js` | texto de fallback demonstrável | ajuda atual é conteúdo, não fonte operacional |
| `wwwroot/js/admin-operation-flow.js` | fluxo operacional simulado | operação publicada usa endpoints reais |
| `wwwroot/js/admin-reviews.js` | avaliações no DemoStore | view atual não carrega o bundle legado |
| `wwwroot/js/admin-stock.js` | saldo/movimento de estoque no DemoStore | `Stock.cshtml` usa `admin-crud.js` e API real |
| `wwwroot/js/admin-subscription.js` | assinatura simulada | módulos comerciais usam API real |

A remoção é segura porque nenhum dos onze nomes de arquivo era referenciado por
uma view, layout ou código servidor. Isso também impede acesso acidental ao asset
estático, mesmo que alguém conheça sua URL anterior.

### Dev-only permitido (4)

| Asset | Motivo objetivo | Proteção |
|---|---|---|
| `wwwroot/js/admin-addons.js` | catálogo/proposta de demonstração | rota `AddOns`: Development + SuperAdmin |
| `wwwroot/js/admin-automations.js` | simulador de automações | rota `Automations`: Development + SuperAdmin |
| `wwwroot/js/admin-integrations.js` | laboratório de conectores | rota `Integrations`: Development + SuperAdmin |
| `wwwroot/js/admin-knowledge-base.js` | ferramenta interna de roteiro | rota `KnowledgeBase`: Development + SuperAdmin |

Esses assets não são fonte de verdade de uma superfície de produção. O próximo
passo, caso se tornem produto, é criar contratos e persistência reais antes de
retirar a guarda; não é permitido apenas promover os bundles atuais.

### Teste/fase final

- `wwwroot/js/tests/demo-store-tests.js` pertence ao laboratório dev e não é um
  teste de aceite de produção.
- `BarberSync.Tests` e `dotnet test` continuam reservados para a fase final.

### Documentação/legacy

Ocorrências em textos de ajuda e nomes históricos descrevem o legado; não são
estado operacional. Views e bundles de Demo Center/Wizard/Diagnostics continuam
permitidos somente sob a mesma guarda Development + SuperAdmin.

### Gerado/build

`node_modules`, lockfiles, source maps e `dist` não integram a medição de código
operacional. Nenhum arquivo gerado foi editado para reduzir artificialmente o
scan.

### Pendente real

Nenhum dos 15 assets operacionais originais permanece pendente. A liberação
continua **NO-GO**, pois builds .NET, banco, smoke autenticado, matriz de escopo e
validação visual ainda exigem o gate Docker completo.

## Escopo anônimo

`EnterpriseDataService` não possui UUID default embutido. Requisições
autenticadas exigem claims UUID válidas; PublicWeb/Totem exigem
`BarberSync:DefaultTenantId` e `BarberSync:DefaultBranchId` configurados como UUID
não vazio. Configuração ausente ou inválida gera erro explícito e o middleware
global é responsável pela resposta sanitizada e correlacionada.

## Achados adicionais corrigidos

A revisão de UUIDs encontrou dois fluxos fora da lista original. O endpoint de
sugestões do Copilot deixou de assumir tenant fixo e de fabricar três sugestões
quando o serviço real retorna vazio. O antigo `PublicConfigController` e seu
`ConfigurationService` foram removidos porque publicavam branding, catálogo,
profissionais e configuração de Totem inteiramente estáticos; nenhuma aplicação
consumia essas rotas, e mantê-las expostas aparentava sucesso operacional sem
persistência. PublicWeb e Totem continuam nos contratos reais baseados em
`EnterpriseDataService` e escopo explícito.

## Sprint de Produção 19 — verificação de regressão

A busca literal solicitada em `Backend`, `Web`, `MobileApp` e `Totem`, excluindo
`node_modules` e `dist`, retornou 161 linhas. As ocorrências remanescentes são as
superfícies dev-only já classificadas, testes browser-side, lockfiles, mensagens
técnicas e legado ainda registrado como pendência; nenhum dos onze bundles
removidos voltou a ser referenciado. A única mensagem de sucesso demonstrativo
encontrada no consumidor legacy do Copilot foi removida: indisponibilidade agora
preserva o erro real, e a consulta exige a claim `tenant_id` da sessão.

A auditoria também confirmou zero referência runtime aos tipos/rota de
configuração pública removidos e zero `script src` local inexistente. O gate não
avançou por ausência do binário Docker, logo a classificação não altera o estado
**NO-GO** nem afirma validação de build, SQL ou API runtime.
