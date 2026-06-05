# Diagnóstico BarberSync

A tela `/Admin/Diagnostics` valida a prontidão demo do AdminWeb sem depender de módulos grandes novos.

## Itens monitorados

- API via `/AdminApi/api-health`.
- AdminApi via `/AdminApi/dashboard`.
- PublicApi representado pelo proxy demo de serviços.
- KioskApi representado pelo proxy/status do totem.
- Swagger via `/AdminApi/swagger.json`.
- Assets principais do Admin.
- DemoStore.
- EventBus.
- localStorage.
- Últimos erros e eventos registrados.

## Ações disponíveis

- **Rodar diagnóstico**: executa checks HTTP e testes JS do DemoStore.
- **Resetar DemoStore**: restaura dados demo e limpa histórico de eventos.
- **Exportar diagnóstico mock**: gera JSON local com resumo e últimos eventos.

## Semáforo

- Verde: OK para demonstração.
- Amarelo: atenção controlada ou validação aguardando execução.
- Vermelho: falha que deve ser corrigida antes da demo.
