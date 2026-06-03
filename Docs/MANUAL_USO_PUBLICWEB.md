# Manual de Uso PublicWeb — Demo Experience 2.0

O PublicWeb foi refinado para conversão comercial com hero premium, CTA fixo, demonstração em passos, antes/depois, resultados esperados, totem em ação, app do cliente, IA na gestão, planos e formulários.

## Fluxo sugerido
1. Abrir `http://localhost:8082/`.
2. Mostrar diferenciais e resultados esperados.
3. Enviar o formulário **Solicitar demonstração comercial**.
4. Enviar o formulário de agendamento público.
5. Explicar que o browser chama apenas `/PublicApi/services`, `/PublicApi/professionals` e `/PublicApi/appointments`.

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
