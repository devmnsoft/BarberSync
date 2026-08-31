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
