# Manual de Uso Totem — Demo Experience 2.0

1. Acesse `http://localhost:8083/Kiosk/Services`.
2. Escolha um serviço em card grande.
3. Informe o cliente com formulário/teclado visual.
4. Selecione o profissional.
5. Confira o resumo lateral.
6. Escolha pagamento PIX, cartão, dinheiro ou misto.
7. Valide o QR fake demonstrativo.
8. Finalize com número de comanda e avaliação por estrelas.

O Totem usa somente `/KioskApi` no browser e possui ajuda, timeout visual, reset automático e modo alto contraste.

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
