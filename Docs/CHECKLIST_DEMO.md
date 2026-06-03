# Checklist Demo — BarberSync Demo Experience 2.0

- [x] Demo mode existe no Admin.
- [x] Demo store persiste alterações em localStorage.
- [x] Demo tour existe com passos, progresso e destaque visual.
- [x] Onboarding, command palette e ajuda contextual permanecem no shell Admin.
- [x] CRUDs possuem ações demonstráveis.
- [x] Dashboard possui insights, alertas e fluxo do dia.
- [x] Cliente 360 existe.
- [x] Agenda muda status via ações demo/fallback.
- [x] Comanda possui pagamento e recibo demonstrativo.
- [x] Caixa/Pagamentos/Financeiro existem.
- [x] Estoque possui reposição inteligente.
- [x] Copilot possui ações.
- [x] PublicWeb possui CTA e formulários.
- [x] Kiosk possui fluxo final com pagamento mock e avaliação.
- [x] Browser não deve chamar URL interna da API; usar `/AdminApi`, `/PublicApi` e `/KioskApi`.
- [ ] Validar Docker, Swagger e logs no ambiente da demonstração.

## BarberSync Demo Experience 3.0

Roteiro comercial integrado:

1. Carregar cenário **Dia Movimentado** no banner do Admin ou em `/Admin/Operations`.
2. Mostrar `/Admin/Dashboard` com métricas vivas de agenda, comandas, receita, estoque, cashback, PublicWeb, Totem e Mobile.
3. Abrir `/Admin/Operations` para conduzir o fluxo Cliente → Agendamento → Check-in → Atendimento → Comanda → Pagamento → Avaliação → Fidelidade.
4. Criar cliente demo, criar agendamento e avançar status até abrir comanda.
5. Pagar com PIX no PDV demo para gerar recibo, baixa de estoque, cashback e avaliação pendente.
6. Abrir `/Admin/Stock` para comprovar estoque reduzido e pedido de reposição demo.
7. Criar campanha pelo Copilot ou por `/Admin/Campaigns` para atualizar indicadores e timeline de clientes.
8. Abrir PublicWeb, solicitar agendamento e apresentar protocolo com CTA para o painel administrativo.
9. Abrir Totem, concluir atendimento, pagamento e `/Kiosk/Summary` com número da comanda.
10. Encerrar mostrando Mobile demo com próximo agendamento, serviços, cashback, promoções, histórico e perfil.

Validações obrigatórias: o navegador usa apenas `/AdminApi`, `/PublicApi` e `/KioskApi`; não há chamada browser direta para `http://api:8080`.

## Como demonstrar o fluxo integrado — BarberSync Integrated Demo 4.0

1. Abra `/Admin/DemoCenter` e carregue o cenário `busyDay`.
2. Crie um cliente demo na Operação do Dia ou em Clientes 360.
3. Crie um agendamento e confirme que ele aparece na Agenda e na Operação do Dia.
4. Faça check-in e observe a mudança para a coluna de check-in.
5. Abra `/Admin/Operations` para conduzir o atendimento no Kanban.
6. Inicie o atendimento e avance o status para em atendimento.
7. Abra uma comanda a partir do card operacional.
8. Adicione um produto, como Pomada Modeladora.
9. Pague a comanda com PIX em modo demonstração.
10. Volte ao Dashboard e valide receita, comandas pagas, eventos recentes e canais.
11. Abra Estoque e valide a baixa automática e a movimentação por comanda.
12. Abra Clientes 360 e valide timeline, pagamento, cashback e avaliação.
13. Abra Copilot e execute a ação de criar campanha de retorno.
14. Use PublicWeb para solicitar agendamento público; o formulário usa `/PublicApi/appointments`, gera protocolo `PUB-2026-0001` e salva `barbersync.public.leads`.
15. Use Kiosk para concluir um atendimento; o totem usa `/KioskApi`, gera atendimento, comanda e pagamento em `sessionStorage`.
16. Abra Relatórios para validar financeiro, agenda, serviços, profissionais, estoque, campanhas, totem, PublicWeb e avaliações.

### Pontos de prova da demo 4.0

- O navegador usa somente proxies locais: `/AdminApi`, `/PublicApi` e `/KioskApi`.
- O estado integrado fica persistido em `localStorage` na chave `barbersync.demo.state.v4`.
- O EventBus emite eventos de cliente, agenda, comanda, pagamento, estoque, cashback, campanhas, avaliações, Copilot e dashboard.
- A Central da Demo exporta/importa JSON, reseta dados e simula importações PublicWeb/Totem quando o armazenamento por porta não é compartilhável.

## Atualização BarberSync Demo Experience 5.0
- Use `/Admin/DemoExperience` como ponto central da apresentação.
- O DemoStore 5.0 persiste fluxos em `barbersync.demo.state.v5` e o EventBus registra eventos em `barbersync.demo.events`.
- Demonstre o fluxo completo: PublicWeb gera protocolo, Admin confirma agenda, Operação realiza check-in, Comanda recebe pagamento, Estoque baixa, Cashback e avaliação são gerados, Campanha e Copilot fecham a narrativa.
- URLs de browser usam proxies locais e rotas públicas de cada aplicação; a URL interna de container da API não é exposta em JavaScript ou Razor.
