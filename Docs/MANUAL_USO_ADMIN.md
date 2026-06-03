# Manual de Uso Admin — Demo Experience 2.0

## Modo demonstração
No topo do Admin existe o banner **Modo Demonstração Ativo** com alternância entre **Demo Comercial**, **API Real** e **Híbrido**. Use os botões para resetar dados ou carregar os cenários: movimentado, estoque crítico, agenda cheia e campanha ativa.

## Tour Demo
Clique em **Tour Demo** na topbar para iniciar um roteiro guiado sem biblioteca externa. O tour destaca Dashboard, KPIs, Agenda, Clientes, Cliente 360, Comanda, Pagamento, Estoque, Campanhas, Copilot e Totem.

## Módulos principais
- **Dashboard:** Resumo da Operação, Fluxo de Hoje, Alertas Prioritários, Oportunidades e Saúde do Negócio.
- **Clientes:** Cliente 360 com segmento, score, cashback, timeline e ações rápidas.
- **Agenda:** visão operacional com mudança de status e toasts.
- **Comandas:** PDV visual com desconto, cupom, cashback, pagamento e recibo.
- **Caixa/Pagamentos/Financeiro:** abertura/fechamento, entradas, saídas, recibos, ticket médio, comissões e fluxo.
- **Estoque:** críticos, atenção, valor total, sugestão de compra e reposição.
- **Campanhas/Cupons/Fidelidade:** funil, público, validade, uso e cashback.
- **Copilot:** sugestões por vendas, estoque, agenda, clientes e financeiro com ação recomendada.

## Ajuda contextual e command palette
Use **Ajuda** para orientação contextual e **Ctrl+K** para abrir a command palette. Todas as ações de demonstração devem responder com alteração visual, toast ou persistência local.

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
