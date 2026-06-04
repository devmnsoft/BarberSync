# Roteiro de demonstração comercial — BarberSync 11.0

## 1. Abertura

Apresente o BarberSync como plataforma SaaS para barbearias, salões, estética e franquias, unificando agenda, PDV, comanda, caixa, estoque, fidelidade, canais digitais e inteligência operacional.

## 2. PublicWeb

1. Abrir `http://localhost:8082/`.
2. Mostrar hero, problemas do mercado, solução e planos.
3. Demonstrar serviços e profissionais carregados via `/PublicApi`.
4. Preencher nome e telefone no formulário de agendamento/proposta.
5. Mostrar protocolo demo e explicar que o lead entra no fluxo comercial.

## 3. AdminWeb

1. Abrir `http://localhost:8081/Admin`.
2. Mostrar dashboard com receita, agenda, cashback, avaliações e status operacional.
3. Navegar por clientes, profissionais, serviços, agenda, comandas, estoque, campanhas, cupons e Copilot.
4. Executar uma ação visual: criar cliente, confirmar agendamento, pagar comanda ou gerar campanha.
5. Explicar DemoStore/localStorage e EventBus como camada de demonstração resiliente.

## 4. KioskWeb

1. Abrir `http://localhost:8083/Kiosk/Services`.
2. Escolher serviço.
3. Informar cliente.
4. Escolher profissional.
5. Confirmar atendimento.
6. Simular pagamento mock.
7. Abrir sucesso, resumo e avaliação.

## 5. Fechamento

Conectar a demonstração aos ganhos comerciais: menos faltas, maior ticket médio, recorrência, controle de estoque, padronização multiunidade e diferenciação por autoatendimento.
