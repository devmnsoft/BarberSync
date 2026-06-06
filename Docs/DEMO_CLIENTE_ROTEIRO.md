# Roteiro de demonstração para cliente - BarberSync 18.0

## Abertura

1. Explique que o BarberSync centraliza agenda, PDV, comandas, estoque, cashback, fidelidade, site público e totem.
2. Mostre o Swagger como evidência da API.
3. Abra o Admin e destaque o Dashboard executivo.

## Diagnóstico

1. Acesse `/Admin/Diagnostics`.
2. Clique **Rodar diagnóstico**.
3. Explique status verde/amarelo/vermelho e o Quality Gate.

## Fluxo principal

1. Acesse `/Admin/FullServiceFlow`.
2. Clique **Validar fluxo completo automaticamente**.
3. Mostre eventos, comanda, pagamento, recibo, cashback, avaliação e dashboard atualizado.

## PublicWeb

1. Acesse `http://localhost:8082/`.
2. Escolha serviço/profissional, informe nome e telefone.
3. Mostre o protocolo demo e explique que o lead entra no Admin.

## Kiosk

1. Acesse `http://localhost:8083/Kiosk/Services`.
2. Selecione serviço, identifique cliente, profissional, confirme e faça pagamento mock.
3. Mostre sucesso, número da comanda e avaliação.

## Fechamento

- Reforce operação omnichannel, fallback demo, prontidão de apresentação e possibilidade de evolução para produção.
