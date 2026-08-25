# Como rodar o Production Readiness local

Este procedimento gera evidência real em uma máquina Windows, sem depender do executor Codex. Ausência de ferramenta, log ou marker obrigatório é **NO-GO**, nunca sucesso ou `SKIPPED`.

## Pré-requisitos Windows

- Git;
- Docker Desktop iniciado, com Docker Compose v2;
- .NET SDK compatível com o projeto;
- Node.js 20+ e npm;
- PowerShell 7 recomendado (Windows PowerShell 5.1 é o mínimo);
- PostgreSQL/`psql`, se for usar banco externo com `-DatabaseUrl`;
- acesso ao repositório clonado.

O script não exibe a connection string. Forneça `-DatabaseUrl` apenas quando necessário; ela será mantida somente no processo e mascarada nos logs/pacote.

## Passo 1: atualizar main

```powershell
git checkout main
git pull origin main
```

## Passo 2: executar preflight

```powershell
.\scripts\run-local-production-readiness.cmd -OnlyPreflight
```

Ferramentas ausentes fazem o preflight falhar claramente. Uma working tree suja gera aviso e é registrada na evidência.

## Passo 3: rodar gate completo

```powershell
.\scripts\run-local-production-readiness.cmd
```

O runner valida contratos, executa o gate Docker oficial, coleta evidências, cria o resumo e empacota o resultado. `-SkipDockerGate` existe apenas para diagnóstico/coleta e necessariamente produz **NO-GO** se os markers reais não existirem.

## Passo 4: gerar pacote

```powershell
.\scripts\package-release-evidence.ps1
```

Também é possível executar somente essa etapa com `.\scripts\run-local-production-readiness.cmd -PackageOnly`. Linux/macOS podem usar `./scripts/package-release-evidence.sh`; sem `zip`, o script cria `tar.gz`.

## Passo 5: enviar para análise

Anexe o ZIP de `artifacts/release-evidence-package/` no ChatGPT/Codex. Não cole credenciais no prompt. Informe branch, commit, máquina e horário usando `docs/RELEASE_EVIDENCE_TEMPLATE.md`.

## Como interpretar GO/NO-GO

Abra primeiro `artifacts/release-evidence/go-no-go.md`. **GO** exige todos os markers `EVIDENCE:*:PASS`. **NO-GO** lista markers falhos e ausentes; arquivos ausentes jamais contam como aprovação.

## Como encontrar o primeiro erro

1. Veja o primeiro item em **Failed markers** e, depois, em **Missing markers** no `go-no-go.md`.
2. Abra o log citado no item dentro de `artifacts/production-readiness/`.
3. Consulte `critical-log-tails.md` para uma visão rápida e `tooling-status.md` para ferramentas ausentes.
4. Corrija a causa e repita o gate completo; não edite markers manualmente.

## Como anexar artifacts no ChatGPT/Codex

Use o botão de anexar arquivo e selecione `barbersync-release-evidence-YYYYMMDD-HHMMSS.zip`. O pacote exclui diretórios de build e arquivos conhecidos de segredo e mascara chaves sensíveis; ainda assim, confirme os avisos do empacotador antes do envio.

## Como fechar PR #201 manualmente

Pela interface: **GitHub > Pull Requests > PR #201 > Close pull request**.

Ou, em uma sessão do GitHub CLI autenticada:

```bash
gh pr close 201 -R devmnsoft/BarberSync --comment "Fechado como obsoleto/superseded. As mudanças de escopo explícito, classificação demo/fallback, remoção dos fallbacks operacionais, isolamento do Copilot, readiness seed, smoke autenticado, POS PASS markers, validação estática e workflow oficial de evidência foram absorvidas e superadas pelos PRs #202 a #212."
```
