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
