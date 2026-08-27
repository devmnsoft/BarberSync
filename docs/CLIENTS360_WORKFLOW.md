# Cliente 360 — fluxo operacional e privacidade

## Visão geral

O Cliente 360 reúne perfil, preferências, restrições, ficha técnica, anamnese, histórico visual, consentimentos, orçamentos, planos e retornos. A rota administrativa `/Clients360` começa por busca nominal e nunca solicita identificadores técnicos. A API deriva `tenant_id`, `branch_id` e usuário exclusivamente das claims autenticadas e registra mutações no log de governança.

## Regras de negócio

### Ficha técnica e anamnese

Toda ficha pertence ao cliente da rota, nasce como `Draft` e pode referenciar profissional, agendamento ou comanda. Versões arquivadas são imutáveis. Anamneses aceitam `Draft`, `Completed`, `Reviewed` e `Archived`; risco `High` aparece como alerta e deve ser confirmado antes de serviço de risco. Respostas sensíveis são separadas e exigem `Clients360.Sensitive.Read`.

### Consentimentos e histórico visual

Termos usam código e versão imutáveis. Somente termo `Active` recebe aceite. Aceite registra canal, data, usuário, IP, user-agent e metadados. Revogação exige motivo e preserva o histórico. Uma foto exige aceite ativo e armazenamento protegido; quando o provider não está configurado, a interface bloqueia o envio e declara a indisponibilidade — nunca simula sucesso. Arquivamento visual exige motivo e substitui exclusão física. Download depende de permissão e não é oferecido pela tela padrão.

Não há reconhecimento facial, biometria, inferência de raça, etnia, idade, gênero ou emoção. Imagens não autenticam usuários, não aparecem em superfícies públicas e não identificam pessoas automaticamente.

### Orçamentos

Um orçamento nasce `Draft`; o total é validado como `subtotal - discount_total`. Itens referenciam serviços/produtos selecionados e preservam preço snapshot. `Presented` pode ser aprovado ou rejeitado, sendo o motivo obrigatório na rejeição. Expirados não convertem. A conversão aprovada apenas prepara a origem `Budget`: o operador confirma a comanda no PDV e não ocorre cobrança automática.

### Planos de tratamento e follow-ups

Planos contêm itens selecionados de serviço/produto. Itens podem gerar agendamento e ligar a comanda concluída. Um plano só termina após os itens pendentes serem tratados; cancelamento exige motivo. Follow-up possui prazo obrigatório, pode nascer de agendamento, orçamento, plano ou ação manual, muda para `Overdue` quando vencido e registra usuário/data ao concluir.

### Preferências e restrições

Preferências são chave/valor por cliente e unidade. Restrições possuem tipo, severidade e vigência. Severidades `High` e `Critical` recebem destaque; uma restrição crítica deve ser confirmada por usuário autorizado antes de serviço incompatível.

## Integrações

- **Agenda:** cada card de agendamento abre o Cliente 360. Perfil, restrição crítica, ficha recente, consentimento e follow-up são consultados antes da execução do serviço.
- **Operação do dia:** o perfil fornece preferências, restrições e linha do tempo sem bloquear o PDV; falha da API produz mensagem amigável com `traceId`.
- **PDV/comanda:** orçamento aprovado preserva snapshots, origem e itens; conversão pede confirmação explícita e nunca cobra automaticamente. Finalização gera evento na timeline e pode completar item de tratamento/criar retorno.
- **Comunicação:** criação e vencimento de follow-up, orçamento apresentado/aprovado, plano criado, consentimento pendente e retorno recomendado podem enfileirar InApp. Canal externo depende de provider, opt-in e suppression list; `ProviderNotConfigured` é erro real.
- **BI Executivo:** métricas incluem clientes com ficha/anamnese, restrições críticas, funil de orçamento, planos, follow-ups, consentimentos e histórico visual. Falta da fonte deve produzir `sourceStatus: unavailable`, nunca zero inventado.
- **Public/Mobile/Kiosk:** contratos públicos expõem somente o próprio cliente e dados mínimos. Mobile pode consultar resumo, consentimentos e follow-ups. Totem aceita termo/check-in mantendo `Kiosk:DeviceCode` obrigatório, sem query string ou fallback. Pré-anamnese pública exige token limitado, expiração e vínculo explícito.

## LGPD e auditoria

Leituras sensíveis e todas as mutações são auditadas com usuário, unidade, entidade, ação e trace. Acesso é tenant/branch scoped. Exclusão física não faz parte do fluxo operacional; solicitações de titular usam governança/LGPD para arquivar, restringir, anonimizar ou reter conforme base legal. Exportação requer `Clients360.Export` e imagens exigem permissão específica.

## Formulários sem ID digitável

Cliente é escolhido por busca; profissional, serviço, produto e termo usam selects pesquisáveis; agenda/comanda usam listas; datas usam pickers; risco usa radio; imagens usam picker protegido. UUIDs existem apenas em `value`, rota ou payload produzido após seleção real. Os formulários carregam `form-validation.js`, usam validação nativa acessível e apresentam erros com `traceId`.

## Permissões

- `Clients360.Read`
- `Clients360.Manage`
- `Clients360.Sensitive.Read`
- `Clients360.VisualRecords.Manage`
- `Clients360.Consents.Manage`
- `Clients360.Budgets.Manage`
- `Clients360.TreatmentPlans.Manage`
- `Clients360.Export`

As permissões complementam autenticação e escopo; não há fallback administrativo, dados demonstrativos em runtime ou sucesso falso.
