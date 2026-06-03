# Checklist Demo BarberSync 6.0

## Técnico
- Swagger abre em `http://localhost:8080/swagger`.
- Admin usa `/AdminApi/...` no browser.
- PublicWeb usa `/PublicApi/...` no browser.
- Kiosk usa `/KioskApi/...` no browser.
- EventBus persiste em `barbersync.demo.events.v6`.
- DemoStore persiste em `barbersync.demo.state.v6`.

## Comercial
- Abrir PublicWeb e criar agendamento.
- Abrir Admin > Lead to Cash e executar fluxo completo.
- Abrir PDV e pagar comanda.
- Conferir Dashboard, Estoque, Cliente 360, Campanhas e Copilot.
- Abrir Totem e concluir autoatendimento demo.

## Validação visual
- SaaS Control Center mostra plataforma, canais, KPIs, ações e eventos.
- Lead to Cash executa fluxo completo e atualiza timeline.
- Channel Manager simula publicação em PublicWeb, Totem e Mobile.
- Dashboard 6.0 reflete conversões, alertas e eventos.
- Cliente 360 mostra LTV, cashback, origem e recomendação Copilot.
- PDV gera pagamento, recibo, cashback e movimentação de estoque.
- Kiosk 5.0 salva sessão e permite impressão mock/reinício.
