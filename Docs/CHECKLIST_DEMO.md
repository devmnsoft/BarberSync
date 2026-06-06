# Checklist de demonstração BarberSync 18.0

## Preparação

- [ ] Rodar `docker compose build --no-cache`.
- [ ] Rodar `docker compose up -d`.
- [ ] Rodar `pwsh -File .\Scripts\quality-gate.ps1`.
- [ ] Conferir `Docs/quality-gate-last-run.md`.

## Roteiro técnico

- [ ] Abrir Swagger: `http://localhost:8080/swagger`.
- [ ] Abrir Admin: `http://localhost:8081/Admin`.
- [ ] Abrir Dashboard: `http://localhost:8081/Admin/Dashboard`.
- [ ] Rodar diagnóstico: `http://localhost:8081/Admin/Diagnostics`.
- [ ] Executar FullServiceFlow automático.
- [ ] Ver Cliente 360 em `/Admin/Clients`.
- [ ] Ver Comanda em `/Admin/ServiceOrders`.
- [ ] Ver Estoque em `/Admin/Stock`.
- [ ] Abrir PublicWeb: `http://localhost:8082/`.
- [ ] Criar agendamento público.
- [ ] Abrir Kiosk: `http://localhost:8083/Kiosk/Services`.
- [ ] Concluir fluxo de autoatendimento.

## Critérios de aceite visual

- [ ] Diagnostics mostra OK ou atenção controlada.
- [ ] FullServiceFlow conclui sem erro crítico.
- [ ] Dashboard atualiza KPIs/eventos.
- [ ] PublicWeb gera protocolo demo.
- [ ] Kiosk mostra número de comanda/sucesso.
- [ ] Console sem `ReferenceError` do BarberSync.
- [ ] Nenhuma chamada browser para host interno Docker.
