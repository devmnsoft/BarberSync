# BarberSync Demo Experience 2.0 — Roteiro Comercial

## Objetivo
Demonstrar a jornada completa do BarberSync para barbearias, salões, estética e franquias: PublicWeb, Admin, agenda, Cliente 360, comanda/PDV, caixa, estoque, campanhas, Copilot, Totem e Mobile.

## Roteiro com tempo estimado
- **2 minutos — PublicWeb:** abrir `http://localhost:8082/`, mostrar hero, CTA fixo, serviços, profissionais, antes/depois, resultados esperados, planos, formulário de demonstração e agendamento público via `/PublicApi`.
- **5 minutos — Admin Dashboard:** abrir `http://localhost:8081/Admin/Dashboard`, ativar o banner demo, selecionar cenários e explicar Resumo da Operação, Fluxo de Hoje, Alertas, Oportunidades e Saúde do Negócio.
- **5 minutos — Clientes/Agenda:** abrir Clientes, mostrar Cliente 360, segmentação, score, próxima melhor ação e depois Agenda para confirmar, check-in, iniciar e finalizar atendimento.
- **5 minutos — Comanda/Caixa:** abrir Comandas, demonstrar PDV visual, desconto, cupom, cashback, pagamento PIX/cartão/dinheiro/misto, recibo e Caixa.
- **3 minutos — Estoque/Campanhas:** abrir Estoque, mostrar produto crítico, reposição e giro; abrir Campanhas/Cupons/Fidelidade para funil, cashback e recorrência.
- **3 minutos — Copilot:** abrir Copilot, usar perguntas rápidas, explicar prioridade, métrica e botão de ação.
- **5 minutos — Totem:** abrir `http://localhost:8083/Kiosk/Services`, escolher serviço, informar cliente, selecionar profissional, confirmar, pagar com QR fake, finalizar e avaliar.

## Pontos de fala
1. O navegador usa proxies relativos: `/AdminApi`, `/PublicApi` e `/KioskApi`.
2. O modo demo mantém a apresentação viva mesmo quando a API real está incompleta.
3. A experiência conecta agenda, comanda, caixa, estoque, campanhas, cashback e Copilot.
4. O Mobile complementa a jornada do cliente com serviços, próximo agendamento, cashback, promoções e perfil.
