# Roteiro de demonstração enterprise BarberSync

Este roteiro valida a experiência comercial integrada do BarberSync sem depender de internet ou de dados externos.

## Portais

- API e Swagger: `http://localhost:8080/swagger`.
- AdminWeb: `http://localhost:8081/Admin`.
- PublicWeb: `http://localhost:8082/`.
- KioskWeb: `http://localhost:8083/Kiosk/Services`.

## Fluxo sugerido

1. Abrir o AdminWeb e apresentar o dashboard executivo com KPIs, agenda, comandas, estoque crítico e sugestões Copilot.
2. Acessar Clientes, Profissionais e Serviços para demonstrar CRUD visual com modal, validação front-end, ações de detalhe, edição e exclusão.
3. Acessar Agenda, Comandas e Estoque para demonstrar operação de atendimento, pagamento simulado e reposição.
4. Abrir o PublicWeb para mostrar a landing premium, serviços, profissionais e formulário público de agendamento via `/PublicApi/appointments`.
5. Abrir o KioskWeb e concluir o fluxo de autoatendimento: serviço, cliente, profissional, confirmação, pagamento mock, sucesso e avaliação.

## Garantias técnicas

- O navegador consome apenas rotas relativas: `/AdminApi`, `/PublicApi` e `/KioskApi`.
- Os proxies MVC chamam a API server-side a partir de `ApiSettings:BaseUrl`.
- Se a API estiver indisponível, os proxies retornam HTTP 200 com payload demo para manter a apresentação funcional.
- Os design systems e assets principais são locais.
