# Runbook de execução do release candidate

Execute os comandos a partir da raiz do repositório. Não promova o candidato sem os artefatos obrigatórios e uma decisão `GO` gerada pelos scripts.

## 1. Fechar o PR #201

Com GitHub CLI autenticado:

```bash
gh pr close 201 -R devmnsoft/BarberSync --comment "Fechado como obsoleto/superseded. As mudanças de escopo explícito, classificação demo/fallback, remoção dos fallbacks operacionais e isolamento do Copilot foram absorvidas e superadas pelos PRs #202, #203 e #204."
```

Alternativa manual:

1. Acesse `GitHub > Pull requests > PR #201 > Close pull request`.
2. Comente: `Fechado como obsoleto/superseded. As mudanças de escopo explícito, classificação demo/fallback, remoção dos fallbacks operacionais e isolamento do Copilot foram absorvidas e superadas pelos PRs #202, #203 e #204.`
3. Confirme que o estado é `Closed` antes de coletar evidências.

## 2. Rodar o gate Docker local

Linux/macOS/Git Bash:

```bash
./scripts/check-release-environment.sh
./scripts/run-production-readiness.sh
./scripts/collect-release-evidence.sh
```

Windows PowerShell:

```powershell
.\scripts\check-release-environment.ps1
.\scripts\run-production-readiness.ps1
.\scripts\collect-release-evidence.ps1
```

A coleta executa novamente o gate para registrar a execução. Se não for desejada uma execução duplicada, rode apenas a pré-checagem e a coleta; a coleta já chama `run-production-readiness`. Preserve `artifacts/production-readiness/` até o resumo ser criado. Não versione `artifacts/release-evidence/`.

## 3. Rodar GitHub Actions

```bash
gh workflow run "Production Readiness" -R devmnsoft/BarberSync
gh run list -R devmnsoft/BarberSync --workflow "Production Readiness" --limit 10
gh run view -R devmnsoft/BarberSync --log
```

No último comando, informe o run quando houver mais de um candidato, por exemplo `gh run view <run-id> -R devmnsoft/BarberSync --log`. Baixe e preserve os artefatos do run. Em seguida execute o coletor local em um checkout do mesmo commit ou preencha `docs/RELEASE_EVIDENCE_TEMPLATE.md`, anexando o link e os logs da execução hospedada.

## 4. Critério de GO

Todos os itens precisam de evidência explícita:

- PR #201 fechado ou inexistente;
- `dotnet restore` OK;
- build Debug OK;
- build Release OK;
- primeira aplicação SQL OK;
- segunda aplicação SQL OK;
- schema validation OK;
- API runtime OK;
- `/health` OK;
- production-smoke OK;
- `node --check` OK;
- Mobile smoke OK;
- Totem smoke OK;
- Totem build OK;
- scan demo/fallback sem pendência operacional crítica.

Execute o parser isoladamente quando os logs já estiverem presentes:

```bash
./scripts/summarize-release-evidence.sh
```

```powershell
.\scripts\summarize-release-evidence.ps1
```

Ausência de log, falha de ferramenta, `Docker is required` ou apenas checks frontend resultam em `NO-GO`. Revise `artifacts/release-evidence/go-no-go.md` e copie o resultado para o template de evidência.

## ProductionReadiness authenticated smoke

The gate replays the canonical schema twice, validates it, and then applies `ScriptsSQL/production_readiness_seed.sql` with the PostgreSQL session setting `barbersync.environment=ProductionReadiness`. The seed refuses every other environment. It creates only the isolated `production-readiness` tenant and fixed readiness identifiers.

Local-only accounts are `admin@readiness.local`, `cashier@readiness.local`, `professional@readiness.local`, and `client@readiness.local`; their disposable password is `ReadinessOnly!2026`. Override it together with the matching seed when operating a private readiness environment. Required variables are `READINESS_ADMIN_EMAIL`, `READINESS_ADMIN_PASSWORD`, `READINESS_CASHIER_EMAIL`, `READINESS_CASHIER_PASSWORD`, `READINESS_PROFESSIONAL_EMAIL`, `READINESS_PROFESSIONAL_PASSWORD`, `READINESS_CLIENT_EMAIL`, `READINESS_CLIENT_PASSWORD`, `READINESS_TENANT_ID`, `READINESS_BRANCH_ID`, and `READINESS_KIOSK_DEVICE_CODE`.

Run `scripts/authenticated-production-smoke.sh http://localhost:5080` or `scripts/authenticated-production-smoke.ps1 -BaseUrl http://localhost:5080`. Password values are never logged. The scripts now create and pay the real readiness service order and require correlated stock, cash, financial, commission and audit persistence; any missing contract/effect exits non-zero rather than emitting a skip.

## Bloqueio estático antecipado

Execute `./scripts/validate-readiness-contracts.sh` (ou `scripts/validate-readiness-contracts.ps1`) antes de Docker, SQL e API. O runner faz isso automaticamente e grava `EVIDENCE:READINESS_CONTRACTS_STATIC:PASS` em `artifacts/production-readiness/readiness-contracts-static.log`. O coletor também registra a saída textual `OK`/`FAIL`. Além dos DTOs, schema, rota e markers POS, o validador confirma escopo tenant/branch nas leituras, campos de correlação do movimento de estoque, escrita de caixa ligada ao pagamento e índices de idempotência dos efeitos financeiro e de comissão. Esta auditoria de contratos C#/SQL/rotas/markers é somente um bloqueio antecipado: não substitui restore/build .NET, PostgreSQL real, smoke autenticado nem o gate completo.
