# Manual de Uso — PublicWeb

## Acesso

- URL local: http://localhost:8082/
- Browser deve chamar apenas `/PublicApi`.

## Seções

- Hero comercial.
- Serviços publicados.
- Profissionais.
- Planos e add-ons.
- Calculadora ROI.
- Formulário de agendamento/proposta.
- CTAs para Admin e Totem.

## Agendamento demo

O formulário valida nome e telefone, envia para `/PublicApi/appointments`, salva histórico em localStorage e mostra protocolo retornado pelo proxy ou protocolo local de fallback.
