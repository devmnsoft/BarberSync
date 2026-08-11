# BarberSync Demo Experience 5.0

## Objetivo
A Demo Experience 5.0 conduz uma demonstração comercial integrada para barbearias, salões, estética e franquias, conectando PublicWeb, Admin, Totem, PDV/comanda, estoque, cashback, avaliações, campanhas, relatórios, Copilot e Mobile.

## Como carregar cenário
1. Acesse `http://localhost:8081/Admin/DemoExperience`.
2. Clique em **Carregar cenário completo** para usar o cenário `dia-movimentado`.
3. Ou escolha um cenário específico: salão novo, estoque crítico, campanha de retorno, totem em uso, alto faturamento, cliente VIP, agenda cheia ou operação com alerta.
4. O estado é gravado em `localStorage` na chave `barbersync.demo.state.v5`.

## Como iniciar tour
1. Clique em **Iniciar demonstração guiada**.
2. A tela executa um fluxo completo: lead, cliente, agenda, check-in, atendimento, comanda, produto, pagamento, recibo, estoque, cashback, avaliação e campanha.
3. Acompanhe a linha do tempo e o painel de eventos recentes.

## Fluxo integrado sugerido
1. Abra o PublicWeb e gere um protocolo de agendamento.
2. No Admin, use Jornada do Cliente para converter lead e criar agendamento.
3. Em Operação ao Vivo, confirme, faça check-in, inicie e finalize atendimento.
4. Em Comandas, adicione serviço/produto e receba pagamento.
5. Mostre recibo, baixa de estoque, cashback e avaliação pendente.
6. Volte ao Dashboard para provar a atualização.

## Demonstrar PublicWeb
- URL: `http://localhost:8082/`.
- Use o formulário de agendamento para gerar protocolo.
- Mostre CTAs para Admin e Totem.

## Demonstrar Totem
- URL: `http://localhost:8083/Kiosk/Services`.
- Execute serviços, cliente, profissional, confirmação, pagamento, sucesso, avaliação e resumo.
- O resumo fica em `sessionStorage`.

## Demonstrar Copilot
- Acesse `http://localhost:8081/Admin/Copilot`.
- Use perguntas sobre movimento, estoque, clientes, profissional e vendas de amanhã.
- Execute a ação sugerida para criar campanha.

## Resetar demo
- Em Demo Experience, clique **Resetar dados demo**.
- Eventos são limpos de `barbersync.demo.events`.
