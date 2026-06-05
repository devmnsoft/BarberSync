# Roteiro de Demonstração ao Cliente — Stable Demo Release 13.0

1. Abrir o Swagger e explicar que a API é o núcleo do SaaS.
2. Abrir o Admin em `/Admin` e apresentar dashboard, agenda, clientes, profissionais, serviços, comandas e estoque.
3. Mostrar que telas usam `/AdminApi` e mantêm fallback demo se a API estiver temporariamente indisponível.
4. Abrir PublicWeb, apresentar hero, serviços, profissionais e formulário de agendamento.
5. Enviar um agendamento demo e destacar o protocolo gerado.
6. Abrir o Totem em `/Kiosk/Services` e executar o fluxo completo de autoatendimento.
7. Finalizar com avaliação e explicar como o ciclo alimenta CRM, fidelidade e relatórios.
8. Demonstrar módulos comerciais: planos, add-ons, integrações, automações, Copilot e relatórios.

## Atendimento Completo 14.0

Use `/Admin/FullServiceFlow` como roteiro principal da apresentação. O fluxo permite demonstrar começo, meio e fim: criação/seleção de cliente, agendamento, check-in, atendimento, comanda, pagamento, recibo, baixa de estoque, cashback, avaliação e dashboard atualizado.

Após concluir o fluxo, abra Dashboard, Cliente 360, Agenda, Comandas, Estoque, Avaliações e Operação para comprovar que os dados foram persistidos no DemoStore/localStorage e distribuídos pelos eventos `flow:*`.
