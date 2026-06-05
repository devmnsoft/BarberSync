# BarberSync Demo Ready Release 17.0 — Relatório de Prontidão

- **Data base:** 2026-06-05.
- **Status geral:** pronto para validação comercial quando executado em ambiente com .NET SDK, PowerShell e Docker disponíveis.
- **Escopo:** API ASP.NET Core, AdminWeb MVC, PublicWeb MVC, KioskWeb MVC, MobileApp React Native, Docker Compose, proxies MVC, DemoStore, EventBus e FullServiceFlow.

## 1. Status geral

O BarberSync foi auditado para a etapa **Demo Ready Release 17.0** com foco em estabilidade, demonstrabilidade e ausência de chamadas diretas do navegador ao host interno Docker `http://api:8080`.

A validação local deste agente confirmou:

- Assets principais existem em AdminWeb, PublicWeb e KioskWeb.
- Código JavaScript de `wwwroot/js` passa em validação sintática com `node --check`.
- MobileApp possui `package.json`, smoke test, tela principal, serviços, tema e telas base.
- `npm install` e `npm test` do MobileApp executaram com sucesso.
- Busca estática nos arquivos de front (`.js`, `.cshtml`, `.html`) não encontrou chamadas diretas para `http://api:8080`, `api:8080`, `localhost:8083/api` ou `8083/api`.

> Observação de ambiente: neste container não há `dotnet`, `docker` nem `pwsh`, portanto as validações de build .NET, Docker Compose, endpoints HTTP reais, Swagger e logs Docker precisam ser executadas no ambiente de demonstração.

## 2. Status API

- Swagger configurado na API via `UseSwagger()` e `UseSwaggerUI()`.
- Health check exposto em `/health`.
- Endpoints essenciais esperados pelo Quality Gate 17.0:
  - `GET /swagger`
  - `GET /swagger/v1/swagger.json`
  - `GET /api/services`
  - `GET /api/professionals`
  - `GET /api/kiosk/services?deviceCode=KIOSK-DEMO-001`
  - `GET /api/kiosk/professionals?serviceId=demo&deviceCode=KIOSK-DEMO-001`

## 3. Status Admin

- Rotas principais mapeadas em `/Admin/*`, incluindo Dashboard, Diagnostics, FullServiceFlow, Clientes, Profissionais, Serviços, Agenda, Comandas, Estoque, Reviews, Operations, Copilot, Reports e Help.
- Dashboard atualizado para **Demo Ready 17.0** com CTA direto para `/Admin/FullServiceFlow` e painel “Status da Demonstração”.
- Proxies AdminApi mantêm fallback demo server-side e preservam o browser usando rotas relativas `/AdminApi/*`.

## 4. Status PublicWeb

- Landing page pública usa `/PublicApi/services`, `/PublicApi/professionals`, `/PublicApi/appointments` e `/PublicApi/leads`.
- Controller PublicApi possui fallback demo para serviços, profissionais, agendamentos, leads e criação de agendamento.
- Sem chamadas browser para host interno Docker.

## 5. Status Kiosk

- Rotas visuais principais existem: `/Kiosk/Services`, `/Kiosk/Client`, `/Kiosk/Professional`, `/Kiosk/Confirm`, `/Kiosk/Payment`, `/Kiosk/Success` e `/Kiosk/Review`.
- Proxies KioskApi possuem fallback demo para serviços, profissionais, cliente, cadastro rápido, pagamento mock e avaliação.
- Fluxo JavaScript usa `sessionStorage` e rotas relativas `/KioskApi/*`.

## 6. Status Mobile

- `MobileApp/package.json` válido.
- `npm install` executado com sucesso.
- `npm test` executou `scripts-smoke-test.js` com sucesso.
- Smoke test verifica arquivos essenciais, imports de tema/API e sintaxe dos módulos principais.

## 7. Status Docker

- `docker-compose.yml` mantém `ApiSettings__BaseUrl=http://api:8080` somente em ambiente server-side dos serviços MVC, o que é permitido.
- Containers esperados:
  - `postgres`
  - `seq`
  - `api`
  - `admin-web`
  - `public-web`
  - `kiosk-web`

## 8. Status Swagger

- API usa Swagger e Swagger UI.
- Quality Gate 17.0 valida `http://localhost:8080/swagger` e `http://localhost:8080/swagger/v1/swagger.json`.

## 9. Status Proxies

Proxies essenciais validados pelo script:

- AdminApi: dashboard, clients, professionals, services, appointments, service-orders, products, stock-critical, campaigns, coupons, reviews, loyalty e copilot-suggestions.
- PublicApi: services, professionals, appointments e leads.
- KioskApi: services, professionals, client/find-by-phone, client/quick-register, payment/mock e review.

## 10. Status FullServiceFlow

- Fluxo cobre cliente, agendamento, check-in, atendimento, comanda, itens, pagamento, recibo, estoque, cashback, avaliação e conclusão.
- Botão **“Rodar fluxo automático de teste”** disponível em `/Admin/FullServiceFlow`.
- Execução automática atualiza DemoStore, emite eventos no EventBus, sincroniza `/AdminApi/full-service-flow/run` com fallback e atualiza dashboard.

## 11. Status DemoStore

- DemoStore possui operações CRUD (`get`, `set`, `add`, `update`, `patch`, `remove`, `reset`, `resetAll`) e funções do FullServiceFlow.
- Persistência específica do fluxo em `barbersync.fullServiceFlow.v14` mantida por compatibilidade com a versão demo existente.
- Fallbacks protegem localStorage vazio.

## 12. Status EventBus

- EventBus expõe `on`, `off`, `emit`, `history` e `clearHistory`.
- Eventos do FullServiceFlow são normalizados e persistidos no histórico local.
- Dashboard e Diagnostics podem consumir histórico recente para apresentação.

## 13. Pendências

Pendências reais deste ambiente:

1. Executar `dotnet build` em máquina com .NET SDK instalado.
2. Executar `dotnet test` em máquina com .NET SDK instalado.
3. Executar `docker compose build --no-cache` em máquina com Docker instalado.
4. Executar `docker compose up -d` e validar endpoints HTTP reais.
5. Executar `pwsh -File Scripts/quality-gate.ps1` em ambiente com PowerShell disponível.
6. Validar logs Docker sem padrões críticos após containers ativos.

## 14. Como demonstrar

1. Subir o ambiente com `docker compose up -d`.
2. Abrir `http://localhost:8080/swagger` e confirmar Swagger.
3. Abrir `http://localhost:8081/Admin`.
4. Abrir `http://localhost:8081/Admin/FullServiceFlow`.
5. Clicar em **Rodar fluxo automático de teste**.
6. Voltar ao Dashboard e mostrar KPIs/eventos atualizados.
7. Abrir `http://localhost:8082/`, simular agendamento público e mostrar protocolo.
8. Abrir `http://localhost:8083/Kiosk/Services`, concluir fluxo de totem com pagamento mock e avaliação.

## 15. Como resetar demo

- No Dashboard, usar **Resetar DemoStore**.
- No FullServiceFlow, usar **Reiniciar fluxo**.
- Manualmente no navegador, limpar as chaves de `localStorage`/`sessionStorage` se necessário:
  - `barbersync.demo.state.v9`
  - `barbersync.fullServiceFlow.v14`
  - chaves do fluxo Kiosk em `sessionStorage`

## 16. Como rodar Quality Gate

No Windows/PowerShell:

```powershell
.\Scripts\quality-gate.ps1
```

No Linux/macOS com PowerShell instalado:

```bash
pwsh -NoLogo -NoProfile -File Scripts/quality-gate.ps1
```

O relatório da última execução é gerado em:

```text
Docs/quality-gate-last-run.md
```
