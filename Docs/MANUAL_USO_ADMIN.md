# Manual de Uso — AdminWeb

## Acesso

- URL local: http://localhost:8081/Admin
- Todas as chamadas do navegador devem usar `/AdminApi`.

## Módulos principais

- Dashboard: KPIs comerciais e operacionais.
- Clients: listagem, criação, edição, detalhes, exclusão e visão 360 demo.
- Professionals: cadastro, performance, status e inativação demo.
- Services: catálogo, preço, duração e canais Site/Totem/Mobile.
- Appointments: criar, confirmar, check-in, iniciar, finalizar e cancelar.
- ServiceOrders: abrir comanda, adicionar itens, pagar, fechar e emitir recibo visual.
- Stock: produtos, entrada, saída, crítico e reposição demo.
- Campaigns/Coupons: ações promocionais e status demo.
- Copilot: perguntas, sugestões e ações demonstrativas.

## Fallback demo

Se a API falhar, o proxy MVC retorna HTTP 200 com dados demonstrativos para não deixar tela vazia.
