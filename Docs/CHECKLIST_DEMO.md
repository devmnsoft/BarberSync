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
