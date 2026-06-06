# Diagnostics Admin BarberSync

## Rota

- `GET /Admin/Diagnostics`

## Validações exibidas

- API via `/AdminApi/api-health`.
- AdminApi via `/AdminApi/dashboard`.
- PublicApi e KioskApi por seus endpoints demo.
- Swagger via `/AdminApi/swagger.json`.
- Assets principais.
- DemoStore, EventBus e localStorage.
- FullServiceFlow local.
- PublicWeb e Kiosk.
- Últimos erros/eventos registrados.
- Testes JS do DemoStore.

## Estados visuais

- Verde: OK.
- Amarelo: atenção controlada/fallback/ambiente indisponível.
- Vermelho: falha real.

A tela nunca fica vazia: todos os cartões iniciam em estado de atenção e são atualizados progressivamente.

## Ações

- **Rodar diagnóstico**: executa endpoints e testes locais.
- **Resetar DemoStore**: limpa estado demo e histórico local.
- **Exportar diagnóstico mock**: baixa JSON com eventos e resumo local.
- Links operacionais para FullServiceFlow, Dashboard e artefatos documentais.
