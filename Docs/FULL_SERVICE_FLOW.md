# BarberSync Full Service Flow 14.0

## Objetivo

O **Atendimento Completo** é o roteiro principal da demonstração BarberSync Vertical Slice Demo 14.0. Ele conecta visualmente Cliente → Agendamento → Check-in → Atendimento → Comanda → Pagamento → Recibo → Baixa de estoque → Cashback → Avaliação → Dashboard atualizado.

## Como executar

1. Suba o ambiente com Docker Compose.
2. Acesse `http://localhost:8081/Admin/FullServiceFlow`.
3. Clique em **Iniciar demo**.
4. Execute cada etapa usando os botões principais da tela.
5. Ao concluir, valide os impactos em Dashboard, Clientes, Agenda, Comandas, Estoque, Avaliações e Operação.

## O que muda no sistema

- Cliente criado ou selecionado é persistido no DemoStore e aparece em Cliente 360.
- Agendamento é salvo com status `Scheduled` e evolui para `CheckedIn`, `InService` e `Finished`.
- Comanda é aberta a partir do agendamento, recebe serviço/produto, desconto, cupom e cashback usado.
- Pagamento marca a comanda como `Paid`, registra pagamento e baixa estoque dos produtos.
- Recibo mock é gerado com número, itens, total, forma de pagamento e mensagem de agradecimento.
- Cashback é calculado em 5% e adicionado ao saldo de fidelidade do cliente.
- Avaliação atualiza nota média, NPS demo e timeline comercial.
- Eventos `flow:*` alimentam histórico, notificações demo, auditoria demo e dashboard.

## Como validar

- `http://localhost:8081/Admin/Dashboard`: KPIs de receita, agenda, cashback e avaliações mudam.
- `http://localhost:8081/Admin/Clients`: seção Cliente 360 mostra agenda, comanda, pagamento, recibo, cashback, avaliação e timeline.
- `http://localhost:8081/Admin/Appointments`: agendamento aparece e reflete status.
- `http://localhost:8081/Admin/ServiceOrders`: comanda do fluxo aparece com itens, pagamento e recibo.
- `http://localhost:8081/Admin/Stock`: movimento de saída por comanda aparece no histórico.
- `http://localhost:8081/Admin/Reviews`: avaliação aparece e recalcula distribuição.
- `http://localhost:8081/Admin/Operations`: card operacional muda de coluna conforme status.

## Como resetar

Na tela `/Admin/FullServiceFlow`, clique em **Reiniciar fluxo**. O estado específico fica em `localStorage` na chave `barbersync.fullServiceFlow.v14` e o DemoStore geral em `barbersync.demo.state.v9`.

## Como apresentar ao cliente

1. Abra o Dashboard e explique que ele reflete a operação ao vivo.
2. Clique em **Executar fluxo completo demo**.
3. Crie o cliente rápido e destaque que ele entra no Cliente 360.
4. Crie o agendamento e mostre a origem via `/AdminApi/services` e `/AdminApi/professionals`.
5. Faça check-in, inicie e finalize atendimento para mostrar Operação ao Vivo.
6. Abra a comanda, adicione serviço/produto e destaque PDV integrado.
7. Pague com PIX/cartão/dinheiro/misto e mostre baixa de estoque.
8. Gere recibo, cashback e avaliação.
9. Volte ao Dashboard para mostrar o impacto consolidado.
