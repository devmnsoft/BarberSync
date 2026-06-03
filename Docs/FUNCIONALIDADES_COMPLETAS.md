# Funcionalidades Completas — BarberSync Demo Experience 2.0

- Modo demo controlado com banner, cenários, persistência local e alternância Demo Comercial/API Real/Híbrido.
- Demo Store com CRUD localStorage para dashboard, clientes, profissionais, serviços, agenda, comandas, produtos, campanhas, cupons, avaliações, fidelidade e Copilot.
- Tour guiado no Admin com 14 passos e destaque visual.
- Command palette, onboarding e ajuda contextual preservados no shell Admin.
- Dashboard 2.0 com operação, fluxo de hoje, alertas, oportunidades e saúde do negócio.
- Cliente 360, segmentação, score, próxima melhor ação e timeline.
- Agenda com status operacional e lista de espera demo.
- Comandas/PDV com desconto, cupom, cashback, pagamento e recibo.
- Caixa, pagamentos e financeiro demonstráveis.
- Estoque com reposição inteligente.
- Campanhas, cupons e fidelidade com funil e cashback.
- Reviews/NPS e recuperação de detratores.
- Copilot 2.0 com sugestões e ações.
- PublicWeb orientado à conversão e agendamento.
- Kiosk com fluxo final, QR fake, acessibilidade e avaliação.
- Mobile com home, serviços, próximo agendamento, cashback, promoções, perfil e CTA de agendar.

## BarberSync Demo Experience 3.0

Roteiro comercial integrado:

1. Carregar cenário **Dia Movimentado** no banner do Admin ou em `/Admin/Operations`.
2. Mostrar `/Admin/Dashboard` com métricas vivas de agenda, comandas, receita, estoque, cashback, PublicWeb, Totem e Mobile.
3. Abrir `/Admin/Operations` para conduzir o fluxo Cliente → Agendamento → Check-in → Atendimento → Comanda → Pagamento → Avaliação → Fidelidade.
4. Criar cliente demo, criar agendamento e avançar status até abrir comanda.
5. Pagar com PIX no PDV demo para gerar recibo, baixa de estoque, cashback e avaliação pendente.
6. Abrir `/Admin/Stock` para comprovar estoque reduzido e pedido de reposição demo.
7. Criar campanha pelo Copilot ou por `/Admin/Campaigns` para atualizar indicadores e timeline de clientes.
8. Abrir PublicWeb, solicitar agendamento e apresentar protocolo com CTA para o painel administrativo.
9. Abrir Totem, concluir atendimento, pagamento e `/Kiosk/Summary` com número da comanda.
10. Encerrar mostrando Mobile demo com próximo agendamento, serviços, cashback, promoções, histórico e perfil.

Validações obrigatórias: o navegador usa apenas `/AdminApi`, `/PublicApi` e `/KioskApi`; não há chamada browser direta para `http://api:8080`.
