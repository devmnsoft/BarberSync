# FullServiceFlow BarberSync

O fluxo completo cobre cliente, agenda, check-in, atendimento, comanda, serviço, produto, pagamento, recibo, estoque, cashback, avaliação e dashboard.

## Execução manual

1. Abrir `/Admin/FullServiceFlow`.
2. Iniciar demo.
3. Criar ou selecionar cliente.
4. Criar agendamento.
5. Confirmar e fazer check-in.
6. Iniciar e finalizar atendimento.
7. Abrir comanda.
8. Adicionar serviço e produto.
9. Registrar pagamento.
10. Gerar recibo.
11. Confirmar estoque.
12. Gerar cashback.
13. Registrar avaliação.
14. Concluir e conferir Dashboard.

## Validação automática

Clique em **Validar fluxo completo automaticamente** para executar o roteiro com dados demo. A rotina:

- Atualiza DemoStore/localStorage.
- Emite eventos pelo EventBus.
- Gera comanda paga, recibo, cashback e avaliação.
- Marca as etapas como concluídas.
- Sincroniza via `/AdminApi/full-service-flow/run` com fallback demo.
- Atualiza o Dashboard.
