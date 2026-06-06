# Diagnostics BarberSync 19.0

## Tela

A rota `/Admin/Diagnostics` mostra status de API, Swagger, AdminApi, PublicApi, KioskApi, assets, DemoStore, EventBus, localStorage, FullServiceFlow, PublicWeb, Kiosk e Docker/Quality Gate.

## Ações disponíveis

- **Rodar diagnóstico**: executa validações browser-side com fallback.
- **Resetar DemoStore**: restaura cenário demo inicial.
- **Exportar diagnóstico**: baixa JSON local com eventos e snapshot.
- **Abrir FullServiceFlow**, **PublicWeb**, **Kiosk** e **Swagger**.
- **Executar novamente** os testes JS do DemoStore.

## Testes JS exibidos

DemoStore inicializa, adiciona cliente, cria agendamento, abre comanda, registra pagamento, gera cashback, cria avaliação, registra EventBus, lê dashboard, persiste localStorage, inicia e conclui FullServiceFlow.
