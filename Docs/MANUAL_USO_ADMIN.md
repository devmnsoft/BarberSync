# Manual de Uso — Admin BarberSync

## Navegação

Acesse `http://localhost:8081/Admin`. O Admin concentra dashboard, clientes, profissionais, serviços, agenda, comandas, estoque, campanhas, cupons, fidelidade, avaliações e Copilot.

## Dashboard

Use o Dashboard Executivo para apresentar receita, agenda, clientes ativos, atendimentos, comandas, ticket médio, estoque crítico, reputação, campanhas, totem e profissionais disponíveis.

## CRUDs demonstráveis

- **Clientes:** criar, editar, ver detalhes, agendar, abrir comanda e excluir.
- **Profissionais:** criar, editar, acompanhar performance e disponibilidade.
- **Serviços:** criar catálogo com preço, duração, comissão e canais Site/Totem/Mobile.
- **Agenda:** criar agendamento e avançar status operacional.
- **Comandas:** abrir, adicionar itens, pagar e fechar.
- **Estoque:** listar produtos, ver críticos e registrar entrada/saída.
- **Relacionamento:** campanhas, cupons, fidelidade e avaliações.
- **Copilot:** perguntas rápidas, sugestões e respostas demo.

## Proxies

Todas as chamadas do Admin no browser usam `/AdminApi`. O proxy MVC chama a API server-side e retorna fallback demo em JSON 200 quando necessário.
