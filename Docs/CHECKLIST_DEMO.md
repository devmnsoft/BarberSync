# Checklist de Demonstração BarberSync 16.0

1. Abrir Swagger: `http://localhost:8080/swagger`.
2. Abrir Admin: `http://localhost:8081/Admin`.
3. Abrir Dashboard e revisar **Status da Demonstração**.
4. Rodar diagnóstico em `/Admin/Diagnostics`.
5. Executar FullServiceFlow automático em `/Admin/FullServiceFlow`.
6. Ver Cliente 360 em `/Admin/Clients`.
7. Ver Comanda em `/Admin/ServiceOrders`.
8. Ver Estoque em `/Admin/Stock`.
9. Abrir PublicWeb: `http://localhost:8082/`.
10. Realizar agendamento público demo.
11. Abrir Kiosk: `http://localhost:8083/Kiosk/Services`.
12. Concluir fluxo do totem até pagamento mock e avaliação.
13. Validar que browser usa apenas `/AdminApi`, `/PublicApi` e `/KioskApi` para comunicação dos front-ends.
14. Executar `Scripts/quality-gate.ps1` antes da apresentação final.
