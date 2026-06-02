# Roteiro de Demonstração BarberSync

Este roteiro conduz uma demonstração comercial ponta a ponta do BarberSync como SaaS para barbearias, salões, estética e franquias.

## URLs da demo

- API/Swagger: `http://localhost:8080/swagger`
- Admin: `http://localhost:8081/Admin`
- PublicWeb: `http://localhost:8082/`
- Totem: `http://localhost:8083/Kiosk/Services`

## Fluxo recomendado

1. Abra o PublicWeb em `http://localhost:8082/` e destaque a proposta comercial, serviços, profissionais, planos e CTAs.
2. Faça um agendamento público no formulário da landing; confirme o toast de sucesso e explique o uso do proxy `/PublicApi/appointments`.
3. Abra o Admin em `http://localhost:8081/Admin`.
4. Acesse o Dashboard Executivo e mostre KPIs, gráficos visuais, comandas, estoque crítico, avaliações e Copilot.
5. Crie um Cliente em `Admin > Clientes`, validando modal, campos obrigatórios, toast e atualização visual.
6. Crie um Serviço em `Admin > Serviços`, marcando canais Site, Totem e Mobile.
7. Crie um Agendamento em `Admin > Agenda` e altere status entre Confirmar, Check-in, Iniciar, Finalizar ou Cancelar.
8. Abra uma Comanda em `Admin > Comandas`.
9. Registre pagamento mock e feche a comanda, destacando recibo visual e kanban.
10. Veja Estoque em `Admin > Estoque` e simule entrada/saída.
11. Crie uma Campanha em `Admin > Campanhas` e mostre resultado/status.
12. Use o Copilot em `Admin > Copilot` com pergunta rápida e histórico local.
13. Abra o Totem em `http://localhost:8083/Kiosk/Services`.
14. Conclua o fluxo do Totem: serviço, cliente, profissional, confirmação, pagamento mock, sucesso e avaliação.

## Mensagens-chave

- O navegador usa somente `/AdminApi`, `/PublicApi` e `/KioskApi`.
- `ApiSettings:BaseUrl` é usado apenas server-side pelos projetos MVC.
- Quando a API real não responde, os proxies retornam fallback demo JSON 200 para preservar a apresentação.
