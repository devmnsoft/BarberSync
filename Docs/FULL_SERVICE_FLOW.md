# FullServiceFlow BarberSync 16.0

O fluxo em `/Admin/FullServiceFlow` valida a jornada completa de atendimento: cliente, agenda, check-in, atendimento, comanda, pagamento, recibo, estoque, cashback, avaliação e dashboard.

## Validação automática

Use o botão **Validar fluxo completo automaticamente** para executar uma massa demo ponta a ponta. A ação:

1. Reinicia o fluxo local.
2. Cria cliente demo.
3. Cria e confirma agendamento.
4. Realiza check-in.
5. Inicia e finaliza atendimento.
6. Abre comanda.
7. Adiciona serviço e produto.
8. Registra pagamento PIX mock.
9. Gera recibo.
10. Confirma baixa de estoque.
11. Gera cashback.
12. Cria avaliação 5 estrelas.
13. Conclui o fluxo, atualiza dashboard e emite `flow:autoValidated`.

Se a API estiver indisponível, o DemoStore/localStorage mantém a demonstração funcionando.
