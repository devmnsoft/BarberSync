# Checklist de demonstração BarberSync 19.0

## Preparação

- [ ] Rodar `docker compose build --no-cache`.
- [ ] Rodar `docker compose up -d`.
- [ ] Rodar `pwsh -File .\Scripts\quality-gate.ps1`.
- [ ] Conferir `Docs/quality-gate-last-run.md`.
- [ ] Abrir `/Admin/DemoWizard` e resetar progresso se necessário.

## Roteiro técnico

- [ ] Abrir Swagger: `http://localhost:8080/swagger`.
- [ ] Abrir Admin: `http://localhost:8081/Admin`.
- [ ] Abrir Demo Wizard: `http://localhost:8081/Admin/DemoWizard`.
- [ ] Rodar diagnóstico: `http://localhost:8081/Admin/Diagnostics`.
- [ ] Executar FullServiceFlow automático.
- [ ] Conferir Dashboard, Cliente 360, Agenda, Comandas, Estoque e Avaliações.
- [ ] Criar agendamento no PublicWeb: `http://localhost:8082/`.
- [ ] Concluir fluxo de autoatendimento no Kiosk: `http://localhost:8083/Kiosk/Services`.

## Critérios de aceite visual

- [ ] Diagnostics mostra OK ou atenção controlada.
- [ ] DemoWizard persiste progresso em `barbersync.demoWizard.progress.v18`.
- [ ] FullServiceFlow conclui sem erro crítico.
- [ ] Dashboard atualiza KPIs/eventos.
- [ ] PublicWeb gera protocolo demo.
- [ ] Kiosk mostra sucesso, comanda e avaliação.
- [ ] Console sem `ReferenceError` do BarberSync.
- [ ] Browser não chama `http://api:8080`.
