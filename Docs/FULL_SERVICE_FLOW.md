# FullServiceFlow BarberSync 18.0

## Objetivo

Validar o atendimento completo de ponta a ponta em modo demo, sem banco real obrigatório.

## Etapas

1. Criar cliente.
2. Criar agendamento.
3. Confirmar e fazer check-in.
4. Iniciar atendimento.
5. Finalizar atendimento.
6. Abrir comanda.
7. Adicionar serviço.
8. Adicionar produto.
9. Realizar pagamento.
10. Gerar recibo.
11. Confirmar estoque.
12. Gerar cashback.
13. Criar avaliação.
14. Concluir e atualizar dashboard.

## Automação

Na rota `/Admin/FullServiceFlow`, clique **Validar fluxo completo automaticamente**. A ação usa dados demo, atualiza DemoStore, emite eventos do EventBus, tenta sincronizar `/AdminApi/full-service-flow/run` e dispara `flow:autoValidated`.

## Persistência

- Estado principal: `barbersync.demo.state.v9`.
- Fluxo completo: `barbersync.fullServiceFlow.v14`.
- Eventos: `barbersync.demo.events.v6`.

## Eventos esperados

`flow:started`, `flow:clientCreated`, `flow:appointmentCreated`, `flow:checkInDone`, `flow:attendanceStarted`, `flow:attendanceFinished`, `flow:serviceOrderOpened`, `flow:itemAdded`, `flow:paymentDone`, `flow:receiptGenerated`, `flow:cashbackGenerated`, `flow:reviewCreated`, `flow:completed`, `flow:autoValidated`.
