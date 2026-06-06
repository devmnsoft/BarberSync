# Roteiro de demonstração para cliente - BarberSync 19.0

## Abertura

1. Explique que o BarberSync centraliza agenda, PDV, comandas, estoque, cashback, fidelidade, site público, totem e Copilot.
2. Mostre o Swagger como evidência da API.
3. Abra `/Admin/DemoWizard` e explique que a apresentação possui roteiro guiado.

## Diagnóstico

1. Acesse `/Admin/Diagnostics` pelo Wizard.
2. Clique **Rodar diagnóstico**.
3. Explique status verde/amarelo/vermelho, DemoStore, EventBus e Quality Gate.

## Fluxo principal

1. Acesse `/Admin/FullServiceFlow`.
2. Clique **Rodar fluxo automático de teste** ou **Validar fluxo completo automaticamente**.
3. Mostre cliente, agendamento, check-in, atendimento, comanda, pagamento, recibo, baixa de estoque, cashback, avaliação e Dashboard atualizado.

## PublicWeb

1. Acesse `http://localhost:8082/`.
2. Escolha serviço/profissional, informe nome e telefone.
3. Mostre protocolo demo e explique entrada do lead/agendamento pelo canal público.

## Kiosk

1. Acesse `http://localhost:8083/Kiosk/Services`.
2. Selecione serviço, identifique cliente, profissional, confirme e faça pagamento mock.
3. Mostre sucesso, número da comanda e avaliação.

## Fechamento

- Reforce operação omnichannel, fallback demo, prontidão comercial, roteiro guiado e próximos passos de implantação.
