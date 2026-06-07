# FullServiceFlow BarberSync 20.0

## Objetivo

Demonstrar o ciclo completo: cliente, agendamento, check-in, atendimento, comanda/PDV, serviço, produto, pagamento, recibo, baixa de estoque, cashback, avaliação e dashboard.

## Execução

1. Acesse `/Admin/FullServiceFlow`.
2. Use o fluxo manual ou clique **Rodar fluxo automático de teste** / **Validar fluxo completo automaticamente**.
3. Confira o evento `flow:autoValidated` no EventBus, em Diagnostics e nos eventos recentes do Dashboard.

## Persistência

- Estado principal do DemoStore: `barbersync.demo.state.v9`.
- Estado do FullServiceFlow: `barbersync.fullServiceFlow.v14`.
- Eventos: `barbersync.demo.events.v6`.

## Eventos esperados

`flow:started`, `flow:clientCreated`, `flow:appointmentCreated`, `flow:checkInDone`, `flow:attendanceStarted`, `flow:attendanceFinished`, `flow:serviceOrderOpened`, `flow:itemAdded`, `flow:paymentDone`, `flow:receiptGenerated`, `flow:cashbackGenerated`, `flow:reviewCreated`, `flow:completed`, `flow:autoValidated`.

## Critérios de aceite do fluxo

- Cada etapa atualiza DemoStore.
- Cada etapa emite EventBus.
- UI mostra feedback/toast.
- Estado persiste em localStorage.
- O fluxo automático conclui sem ReferenceError e gera relatório final demo.
