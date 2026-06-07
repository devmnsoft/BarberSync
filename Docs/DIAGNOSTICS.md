# Diagnostics BarberSync 20.0

## Tela

A rota `/Admin/Diagnostics` mostra status de API, Swagger, AdminApi, PublicApi, KioskApi, assets, DemoStore, EventBus, localStorage, FullServiceFlow, PublicWeb, Kiosk e Docker/Quality Gate.

## Ações disponíveis

- **Rodar diagnóstico**: executa validações browser-side com fallback controlado.
- **Resetar DemoStore**: restaura cenário demo inicial.
- **Exportar diagnóstico**: baixa JSON local com eventos, snapshot e versão-alvo 20.0.
- **Abrir FullServiceFlow**, **Abrir PublicWeb**, **Abrir Kiosk** e **Abrir Swagger**.
- **Executar novamente** os testes JS do DemoStore.

## Testes JS exibidos

DemoStore inicializa, adiciona cliente, cria agendamento, abre comanda, registra pagamento, gera recibo, gera cashback, cria avaliação, registra EventBus, lê dashboard, persiste localStorage, inicia e conclui FullServiceFlow.

## Estados visuais

- Verde: OK.
- Amarelo: atenção controlada ou dependência externa indisponível.
- Vermelho: falha crítica de rota, asset, proxy ou recurso local.
