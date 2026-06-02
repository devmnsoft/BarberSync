# BarberSync — Manual de Uso Admin

## Primeiros passos
Acesse `/Admin/Onboarding` para acompanhar o checklist salvo em `localStorage`: empresa, serviços, profissionais, clientes, agenda, comanda, totem, site público, Copilot e roteiro.

## Dashboard
Use `/Admin/Dashboard` para apresentar KPIs, serviços mais vendidos, próximos agendamentos, estoque crítico, sugestões do Copilot, insights para hoje e plano de ação recomendado.

## Cadastros e operação
- **Clientes**: CRUD visual, busca/filtro, Cliente 360, agendamento e abertura de comanda.
- **Profissionais**: CRUD visual, inativação demo e performance com receita, comissão, avaliação, ocupação e meta.
- **Serviços**: CRUD visual e toggles para PublicWeb, Totem e Mobile.
- **Agenda**: mudança de status com chamada a `/AdminApi/appointments/{id}/...` e atualização local demo.
- **Comandas**: detalhe, pagamento, fechamento e recibo visual.
- **Estoque**: entrada/saída demo, barras, status crítico/atenção/normal e sugestão de compra.

## Comercial e relacionamento
Campanhas, cupons, fidelidade e avaliações possuem dados demo ricos para mostrar retorno, uso, cashback, NPS e recuperação de detratores.

## Copilot
Use `/Admin/Copilot` para perguntar sobre retenção, escala, estoque, campanha e reputação. As respostas possuem prioridades e ações rápidas.

## Estado local demo
As telas administrativas usam o `BarberSyncDemoStore` no front-end para persistir alterações em `localStorage`. Use “Atualizar” para comprovar que criações, edições, exclusões e mudanças de status continuam visíveis durante a demonstração.
