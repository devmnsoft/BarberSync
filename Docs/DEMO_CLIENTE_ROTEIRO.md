# Roteiro de demonstração comercial BarberSync

## Sequência recomendada

1. Abrir o PublicWeb em `http://localhost:8082/` e apresentar a proposta premium para barbearias, salões, estética e franquias.
2. Mostrar serviços e profissionais carregados por `/PublicApi/services` e `/PublicApi/professionals`.
3. Simular um agendamento público pelo formulário da landing; o navegador chama somente `/PublicApi/appointments`.
4. Entrar no Admin em `http://localhost:8081/Admin`.
5. Abrir o Dashboard executivo e destacar KPIs, estoque crítico, agenda, comandas e Copilot.
6. Cadastrar um cliente em Clientes, validar campos obrigatórios, salvar e exibir toast.
7. Cadastrar ou editar um serviço e destacar publicação em site, totem e mobile.
8. Criar um agendamento e demonstrar ações de confirmar, check-in, iniciar, finalizar e cancelar.
9. Abrir uma comanda, adicionar fluxo de pagamento e demonstrar o recibo visual.
10. Ver estoque, alertas críticos e simular entrada/saída.
11. Mostrar campanhas, cupons, fidelidade e avaliações.
12. Usar o Copilot para pedir uma sugestão comercial.
13. Abrir o Totem em `http://localhost:8083/Kiosk/Services`.
14. Fazer o fluxo do Totem: serviço, cliente, profissional, confirmação, pagamento mock, sucesso e avaliação.
15. Fechar reforçando que PublicWeb, AdminWeb e KioskWeb usam proxies MVC e nunca expõem o host interno Docker da API no browser.

## Mensagem comercial

O BarberSync demonstra uma operação omnichannel para beleza: agenda, CRM, checkout, estoque, reputação, cashback, campanhas, Totem e MobileApp com uma identidade visual única e fallback demo para apresentações mesmo quando a API de produção estiver indisponível.
