# Manual de uso do Admin BarberSync

## Navegação

O Admin possui sidebar escura, topbar com busca global, status de API, unidade atual e usuário Admin Demo. As telas usam `/AdminApi/*` como proxy server-side.

## Dashboard

Acesse `/Admin/Dashboard` para KPIs executivos, serviços mais vendidos, profissionais, próximos agendamentos, comandas abertas, estoque crítico, alertas e sugestões Copilot.

## CRUDs principais

Clientes, Profissionais e Serviços possuem busca, cards de resumo, tabela, detalhes, criar, editar, excluir com confirmação, validação HTML5 e toasts. Em modo demo, POST/PUT/DELETE retornam sucesso para manter a demonstração fluida.

## Operação

- Agenda: criar agendamento e demonstrar status confirmar, check-in, iniciar, finalizar e cancelar.
- Comandas: abrir comanda, pagar e fechar.
- Estoque: listar produtos, ver críticos e registrar entrada/saída.
- Relacionamento: campanhas, cupons, fidelidade e avaliações.
- Copilot: sugestões automáticas e chat por `/AdminApi/copilot/ask`.

## Segurança de integração

JavaScript chama apenas rotas relativas. O host interno da API é usado somente pelos controllers MVC via `ApiSettings:BaseUrl`.
