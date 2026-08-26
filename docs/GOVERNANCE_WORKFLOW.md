# Governança SaaS

## Escopo e isolamento
Todas as rotas `/api/governance` exigem autenticação, permissões e claims `tenant_id`/`branch_id`. O tenant corrente só consulta sua própria empresa; filial é obrigatória nos recursos locais. SuperAdmin usa o escopo assinado da sessão, sem aceitar tenant digitado no payload.

## Empresas, filiais, planos e módulos
A central lista a empresa do escopo, gerencia filiais, consulta planos e assinatura e habilita módulos persistidos. `IPlanLimitService` lê a assinatura real e bloqueia criação/ativação acima dos limites com `upgradeRequired`; leituras permanecem disponíveis. Ausência de assinatura é erro de configuração, nunca plano simulado.

## Usuários, convites e permissões
Convites exigem e-mail, perfil e filial escolhidos nas listas. O token aleatório é devolvido somente ao integrador de entrega e apenas o SHA-256 é persistido; logs não contêm token. Perfis têm código único validado e a matriz usa nomes nas colunas e checkboxes. Mudanças ficam disponíveis para auditoria de governança.

## Segurança, auditoria e privacidade
Políticas cobrem sessão, convite, tentativas e senha forte. MFA permanece `NotConfigured`, sem simulação. Eventos de segurança e auditoria são filtráveis por usuário selecionado, módulo, ação, severidade e período. Metadados nunca devem conter senha, token, JWT, chaves, connection string ou `DATABASE_URL`. Solicitações LGPD seguem estados auditáveis; nenhuma rota apaga dados diretamente. Exports começam em `Pending` e somente um processador real pode concluir ou publicar arquivo.

## Onboarding
O checklist possui dez itens: filial, serviços, profissionais, agenda, caixa, estoque, pagamentos, permissões, comunicação e IA opcional. A UI calcula progresso a partir dos itens persistidos, oferece atalhos e exige motivo para pular. Integrações podem concluir itens ao detectar dados reais.

## Formulários e efeitos colaterais
As páginas usam `form-validation.js`, validação nativa/inline, loading, bloqueio de submit duplicado e erro com `traceId`. Empresa, filial, titular, perfil, plano e módulos são selecionados por dropdown, autocomplete, radio ou checkbox; UUIDs existem apenas no `value`/payload depois da seleção. Escritas geram persistência real e não possuem fallback local.

## Permissões
`Governance.Read`, `Governance.Manage`, `Tenant.Manage`, `Branch.Manage`, `Users.Manage`, `Roles.Manage`, `Permissions.Manage`, `Plans.Manage`, `Subscription.Manage`, `Security.Read`, `Security.Manage`, `Privacy.Manage`, `Exports.Manage` e `Onboarding.Manage` separam leitura e ações sensíveis.

## Limitações conhecidas
MFA e logout global não estão implementados e são apresentados como `NotConfigured`. Entrega de convite e geração física de export dependem de workers/providers configurados; a API registra o workflow sem declarar sucesso externo.
