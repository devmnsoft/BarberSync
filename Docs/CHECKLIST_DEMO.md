# Checklist de demonstração BarberSync 20.0

## Preparação

- [ ] Rodar `dotnet build`.
- [ ] Rodar `dotnet test`.
- [ ] Rodar `docker compose build --no-cache`.
- [ ] Rodar `docker compose up -d`.
- [ ] Rodar `docker compose ps`.
- [ ] Rodar `pwsh -File .\Scripts\quality-gate.ps1`.
- [ ] Conferir `Docs/quality-gate-last-run.md`.

## Roteiro técnico

- [ ] Abrir Swagger: `http://localhost:8080/swagger`.
- [ ] Abrir Admin: `http://localhost:8081/Admin`.
- [ ] Rodar diagnóstico: `http://localhost:8081/Admin/Diagnostics`.
- [ ] Executar FullServiceFlow automático: `http://localhost:8081/Admin/FullServiceFlow`.
- [ ] Abrir Dashboard: `http://localhost:8081/Admin/Dashboard`.
- [ ] Conferir Cliente 360, Agenda, Comandas, Estoque e Avaliações.
- [ ] Criar agendamento no PublicWeb: `http://localhost:8082/`.
- [ ] Concluir fluxo de autoatendimento no Kiosk: `http://localhost:8083/Kiosk/Services`.

## Critérios de aceite visual

- [ ] Diagnostics mostra OK ou atenção controlada.
- [ ] FullServiceFlow conclui sem erro crítico.
- [ ] Dashboard atualiza KPIs/eventos.
- [ ] Cliente 360 mostra histórico.
- [ ] Comanda gera recibo.
- [ ] Estoque baixa produto vendido.
- [ ] PublicWeb gera protocolo demo.
- [ ] Kiosk mostra sucesso, comanda e avaliação.
- [ ] Console sem `ReferenceError` do BarberSync.
- [ ] Browser não chama `http://api:8080`.
