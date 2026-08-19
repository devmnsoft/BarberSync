# BarberSync 2.0 — checklist de prontidão para produção

Atualizado em 19 de agosto de 2026. Este checklist é um gate de publicação: um
item sem evidência permanece reprovado, mesmo quando a implementação existe no
repositório.

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

### Evidência parcial desta sprint

- [x] Controladores que simulavam pagamento, estoque, comanda e auditoria em memória removidos.
- [x] Telas ainda dependentes de armazenamento local isoladas em `Development` + `SuperAdmin`.
- [x] Estoque protegido explicitamente por autenticação e `Stock.View`, `Stock.Entry` ou `Stock.Adjust`.
- [ ] SDK .NET disponível no executor (`dotnet --info`: comando ausente).
- [ ] PostgreSQL disponível no executor (`psql --version`: comando ausente).
- [ ] As 347 correspondências brutas restantes da varredura foram classificadas e resolvidas.

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
