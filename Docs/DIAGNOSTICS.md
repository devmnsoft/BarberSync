# Diagnóstico BarberSync 16.0

A tela `/Admin/Diagnostics` é o painel visual de saúde da demonstração. Ela não depende da API para renderizar conteúdo inicial e usa cards com status verde, amarelo ou vermelho.

## Itens exibidos

- API via `/AdminApi/api-health`.
- AdminApi via `/AdminApi/dashboard`.
- PublicApi via `http://localhost:8082/PublicApi/services`.
- KioskApi via `http://localhost:8083/KioskApi/services?deviceCode=KIOSK-DEMO-001`.
- Swagger via `/AdminApi/swagger.json`.
- Assets Admin.
- DemoStore.
- EventBus.
- localStorage.
- FullServiceFlow.
- PublicWeb.
- Kiosk.
- Docker como validação manual/demo pelo Quality Gate.

## Ações

- **Rodar diagnóstico**: executa probes HTTP e testes locais.
- **Resetar DemoStore**: limpa dados demo e recalcula status.
- **Exportar diagnóstico mock**: baixa JSON com eventos e resumo local.
- **Executar testes JS**: valida DemoStore, EventBus, localStorage e FullServiceFlow no navegador.
