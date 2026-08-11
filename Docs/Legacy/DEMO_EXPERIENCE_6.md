# BarberSync SaaS Demo 6.0

A Demo 6.0 apresenta uma jornada SaaS ponta a ponta: PublicWeb capta lead, Lead to Cash converte em cliente, Operação faz check-in/atendimento, PDV recebe comanda, Caixa registra pagamento, Estoque baixa produto, Fidelidade gera cashback, Avaliações registram satisfação, Campanhas reativam clientes, Copilot sugere ações, Totem simula autoatendimento e Dashboard consolida tudo.

## Fluxos principais
- Lead to Cash: lead público → cliente → agendamento → atendimento → comanda → pagamento → fidelidade → avaliação.
- Totem 5.0: serviço → combo → cliente → profissional → confirmação → pagamento → sucesso → avaliação.
- PDV 3.0: carrinho, serviços, produtos, desconto, cupom, cashback, recibo e baixa de estoque.
- Campanhas 3.0: segmentação, campanha, cupom e resultado demo.
- Copilot 4.0: diagnóstico contextual baseado no DemoStore v6.

## Canais e proxies
O navegador não deve chamar `http://api:8080`. Admin usa `/AdminApi`, PublicWeb usa `/PublicApi` e Totem usa `/KioskApi`; `ApiSettings:BaseUrl` permanece server-side.

## Cenários DemoStore
Cenários suportados: `default`, `newSalon`, `busyDay`, `emptyDay`, `criticalStock`, `highRevenue`, `campaignMode`, `kioskDemo`, `publicWebLeads`, `franchiseDemo`, `vipClients` e `stockEmergency`.

## Auditoria visual
Todos os módulos críticos emitem eventos persistidos no EventBus v6: lead, agendamento, check-in, atendimento, comanda, pagamento, estoque, cashback, avaliação, campanha, cupom, Copilot e refresh de dashboard.
