# BarberSync CommercialFlow 15.0

## Objetivo

A tela `/Admin/CommercialFlow` é o roteiro central da demonstração comercial BarberSync Vertical Slice + Produto Comercial 15.0. Ela conecta PublicWeb, Admin, Totem e Mobile com cliente, agenda, check-in, atendimento, comanda, pagamento, recibo, estoque, cashback, avaliação, dashboard e relatórios demo.

## Como executar

1. Suba a solução com Docker ou localmente.
2. Abra `http://localhost:8081/Admin/CommercialFlow`.
3. Escolha a origem: Admin, PublicWeb, Totem ou Mobile.
4. Crie ou selecione um cliente.
5. Crie o agendamento com serviço/profissional carregados por `/AdminApi/services` e `/AdminApi/professionals` com fallback demo.
6. Realize check-in, inicie e finalize atendimento.
7. Abra a comanda, adicione serviço/produto, aplique desconto, cupom e cashback usado.
8. Registre pagamento PIX/cartão/dinheiro/misto.
9. Confira recibo visual, mock de PDF/compartilhamento e QR fake.
10. Confirme cashback e salve avaliação.
11. Abra Dashboard, Cliente 360, Agenda, Comandas, Estoque, Avaliações e Operação ao Vivo para comprovar a integração.

## O que o fluxo altera

- `barbersync.commercialFlow.v15` no `localStorage`.
- `clients`: cliente rápido ou cliente existente com timeline.
- `appointments`: agendamento com origem e status operacional.
- `attendances`: check-in, atendimento iniciado e finalizado.
- `serviceOrders`: comanda aberta e paga.
- `payments`: pagamento aprovado em modo demonstração.
- `receipts`: recibo visual da comanda.
- `products` e `stockMovements`: baixa de produto somente após pagamento.
- `loyalty`: cashback gerado/confirmado.
- `reviews`: avaliação e NPS demo.
- `dashboardEvents`, notificações e relatórios de auditoria demo via EventBus.

## Eventos principais

O EventBus registra eventos `commercialFlow:*`: `started`, `originSelected`, `clientSelected`, `appointmentCreated`, `checkInDone`, `attendanceStarted`, `attendanceFinished`, `serviceOrderOpened`, `paymentDone`, `receiptGenerated`, `cashbackGenerated`, `reviewCreated` e `completed`.

## Como resetar

Na tela `/Admin/CommercialFlow`, clique em **Recomeçar fluxo**. Tecnicamente, o método `BarberSyncDemoStore.resetCommercialFlow()` remove `barbersync.commercialFlow.v15` e cria um novo fluxo.

## Como validar

- Verifique que o browser usa somente `/AdminApi`, `/PublicApi` e `/KioskApi`.
- Confirme que não existem chamadas browser para `http://api:8080`.
- Execute `dotnet build`.
- Suba o Docker Compose e valide Swagger, Admin, PublicWeb e Kiosk.
- Valide os proxies `/AdminApi/dashboard`, `/AdminApi/clients`, `/AdminApi/professionals`, `/AdminApi/services`, `/AdminApi/appointments`, `/AdminApi/service-orders`, `/AdminApi/products`, `/PublicApi/services`, `/PublicApi/professionals`, `/KioskApi/services?deviceCode=KIOSK-DEMO-001`.

## Como apresentar ao cliente

Use a narrativa: “um lead chega pelo PublicWeb ou Totem; a recepção seleciona/cria o cliente; o sistema agenda, recebe check-in, acompanha atendimento, abre comanda, recebe pagamento, baixa estoque, gera cashback, coleta avaliação e atualiza dashboard/relatórios”. Finalize mostrando a próxima melhor ação no Resultado.
