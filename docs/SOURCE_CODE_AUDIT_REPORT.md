# Relatório de auditoria geral do código-fonte

- **Data:** 2026-08-31 (UTC)
- **Branch analisada:** `work`
- **Commit base:** `0a392916f4a003fdad57396c3f255e9772f05107` (merge do PR #234)
- **Escopo:** Backend, Web/AdminWeb, PublicWeb, KioskWeb, MobileApp, Totem, ScriptsSQL, scripts, Scripts, docs e workflows.

## Ferramentas e comandos

Foram executados `git status --short --branch`, os cinco levantamentos obrigatórios com `rg`, inspeção de rotas/assets/permissões, `node --check`, validadores estáticos, builds npm e validações de YAML/readiness. Os levantamentos retornaram, respectivamente, **302**, **757**, **1.308**, **485** e **2.035** linhas para triagem; ocorrências legítimas (por exemplo, `catch` com tratamento e menções documentais) não foram classificadas automaticamente como defeito.

## Achados reais e correções

| Severidade | Achado | Risco | Correção / arquivos |
|---|---|---|---|
| High | Não existia contrato/API/DI para Central de Controle, apesar da necessidade de consolidação operacional. | Rotas inexistentes e impossibilidade de operar alertas/incidentes. | Controllers tenant/branch scoped e contratos validados em `CommandCenterControllers.cs`; usa o `TeamDataService` já registrado. |
| High | Não havia persistência idempotente para alertas, incidentes, tarefas, snapshots, widgets e checks. | Falha em runtime/SQL ao ativar o módulo. | Seis tabelas, constraints e índices aditivos em `script_completo.sql`. |
| Medium | Permissões `CommandCenter.*` não estavam seedadas. | 403 permanente para perfis não administrativos. | Seis permissões e vínculo idempotente ao papel operacional. |
| Medium | Readiness não verificava referências, Razor, IDs técnicos, permissões e sintaxe JS como um gate único. | Regressões de UI/contrato chegariam ao release. | Validadores sh/PowerShell e integração ao readiness. |
| Medium | Admin não tinha superfície integrada nem estados honestos para fonte indisponível. | Operador sem visibilidade e risco de health inventado. | Views, JS e CSS com vazio/loading/erro/traceId e estados `Unknown`/`NotConfigured`. |
| Low | Contratos de rotas e workflow da Central não existiam. | Drift entre API, UI e operação. | Documentação criada/atualizada. |

**Critical encontrados:** nenhum confirmado na triagem. Não foram removidas autenticação, autorização ou validações.

## Pendências conhecidas

- A busca ampla contém dívida histórica (mensagens “em breve”, armazenamento local em experiências isoladas e catches tratados). **Risco Low/Medium**, conforme o fluxo; não foi feita substituição indiscriminada porque isso poderia apagar funcionalidades.
- A matriz somente declara `Healthy` quando um check real persistido assim o informa. Provedores não medidos permanecem `Unknown`/`NotConfigured`. **Risco controlado**.
- Validação SQL contra PostgreSQL depende de `psql` e de banco local configurado. **Risco Medium de ambiente**, coberto por SQL idempotente e gate quando disponível.

## Evidências

O gate produz `EVIDENCE:SOURCE_INTEGRITY_STATIC:PASS`; o contrato UI preserva `EVIDENCE:UI_CONTRACTS_STATIC:PASS`. Builds e resultados finais são registrados no resumo da sprint/PR. Nenhum arquivo em `Backend/Tests/BarberSync.Tests` foi alterado e `dotnet test` não foi executado.

## Sprint 57 — Auditoria de consistência comercial, preço, margem, desconto, comissão e catálogo

### Evidência executada

Foram executadas as três buscas obrigatórias por preço/total/desconto/comissão/benefícios, conversões numéricas e padrões inseguros em `Backend`, `Web`, `MobileApp`, `Totem`, `ScriptsSQL` e `docs` (excluindo artefatos). Resultado bruto: 3.708 referências comerciais, 346 referências numéricas e 167 referências para triagem de padrões/fallbacks.

### Constatações e correções

- A base já protege gift card e wallet contra saldo negativo, deduplica comissões de parceiro por origem, confirma payout com pagamento real e valida igualdade entre subtotal, desconto e total de orçamento. Esses controles foram preservados.
- Preço de serviço/produto estava disperso entre entidades operacionais, estoque, site público e marketplace, sem perfil comercial único. Foram adicionados perfis tenant/branch scoped, custo, margem, duração/estoque e visibilidades.
- Não havia breakdown central versionável. `CatalogPricingService` passou a calcular com `decimal`, arredondamento monetário explícito, prioridade, preço mínimo e alerta/aprovação de margem.
- Combos/pacotes legados não ofereciam snapshots comerciais completos. As novas definições exigem itens e quantidades positivas, preço não negativo, validade positiva e bloqueio de ativação de combo vazio.
- Comissão de catálogo agora retorna `Pending` para origem sem evento real e só prevê `Payable` nos gatilhos aceitos; unicidade SQL evita cálculo duplicado.
- Publicação Mobile filtra status, visibilidade e estoque. Admin usa somente opções reais; nenhum ID técnico é digitável.

### Pendências controladas

Agenda, PDV, PublicWeb, Kiosk e BI ainda possuem contratos legados que não podem ser removidos de modo destrutivo. A migração progressiva deve trocar suas leituras pelo catálogo central, mantendo compatibilidade até todas as instalações aplicarem a versão de schema 033. Cashback legado não possui uma coluna universal de expiração; novos benefícios devem usar validade explícita e essa migração permanece registrada para sprint de dados dedicada. Nenhum pagamento, comissão paga ou fallback foi fabricado para encobrir essas pendências.

## Sprint 58 — Auditoria operacional, checkout, comanda, pagamento, estoque e comissão

Foram executadas as quatro varreduras obrigatórias de regras operacionais, tipos/conversões, integridade de UI/fallbacks e DI/autorização (1.036, 331, 210 e 1.345 ocorrências brutas, respectivamente). A revisão confirmou módulos legados de `ServiceOrders`, caixa, pagamentos, estoque e catálogo, mas não encontrou uma orquestração única de execução/checkout.

### Inconsistências confirmadas e correções

- Não existiam serviços explícitos para check-in idempotente, checkout consultivo/transacional, allocation/reversão, consumo/reversão, accrual idempotente e sessão de caixa. Foram consolidados em serviços scoped.
- O fluxo legado não possuía snapshots operacionais próprios. O schema agora preserva preço, consumo, accrual, checkout, caixa e eventos sem `DROP`, `TRUNCATE` ou exclusão do movimento original.
- O risco de `Pending` fechar comanda foi bloqueado: confirmação resolve o estado persistido do pagamento e aceita apenas `Confirmed`.
- Concorrência de check-in, checkout ativo, intenção, caixa aberto e comissão agora possui condições/índices únicos; saldo insuficiente bloqueia consumo.
- A UI nova usa selects originados de opções reais, nunca IDs técnicos digitáveis, e contém loading, vazio e ProblemDetails/traceId.
- Valores novos usam `decimal`/`numeric`; dados externos chegam tipados pelo model binding, sem `Guid.Parse`.

### Pendências/sourceStatus

Wallet, gift card, voucher, coupon, cashback, pacote e clube têm origem validada e deduplicação por sessão, mas o débito atômico de cada ledger continua pertencendo aos stores de Clube & Vendas; a integração deve ocorrer antes de ativar cada benefício em produção. Gateway continua externo e nenhum endpoint converte intenção `Pending` em confirmada. Workflow Studio consome eventos persistidos quando instalado (`persisted-awaiting-consumer`). Kiosk não foi relaxado: `DeviceCode` permanece obrigatório na fronteira existente. O seed não fabrica payment/allocation, baixa de estoque ou comissão.
