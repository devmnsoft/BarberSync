# Roteiro de demonstração para cliente - BarberSync 20.0

## Abertura

1. Explique que o BarberSync centraliza agenda, PDV, comandas, estoque, cashback, fidelidade, site público, totem e Copilot.
2. Mostre o Swagger como evidência da API em `http://localhost:8080/swagger`.
3. Abra o Admin em `http://localhost:8081/Admin`.

## Diagnóstico

1. Acesse `/Admin/Diagnostics`.
2. Clique **Rodar diagnóstico**.
3. Explique status verde/amarelo/vermelho, DemoStore, EventBus, localStorage, FullServiceFlow e Quality Gate.
4. Use os botões para abrir Swagger, PublicWeb, Kiosk e FullServiceFlow.

## Fluxo principal

1. Acesse `/Admin/FullServiceFlow`.
2. Clique **Rodar fluxo automático de teste** ou **Validar fluxo completo automaticamente**.
3. Mostre cliente, agendamento, check-in, atendimento, comanda, pagamento, recibo, baixa de estoque, cashback, avaliação e Dashboard atualizado.

## PublicWeb

1. Acesse `http://localhost:8082/`.
2. Escolha serviço/profissional, informe nome e telefone.
3. Mostre protocolo demo e explique entrada do lead/agendamento pelo canal público via `/PublicApi`.

## Kiosk

1. Acesse `http://localhost:8083/Kiosk/Services`.
2. Selecione serviço, identifique cliente, profissional, confirme e faça pagamento mock.
3. Mostre sucesso, número da comanda e avaliação via `/KioskApi`.

## Fechamento

- Reforce operação omnichannel, fallback demo, prontidão comercial, quality gate, diagnostics e próximos passos de implantação.
