# Manual do PublicWeb — BarberSync 8.0

## Acesso

Abra `http://localhost:8082/`.

## Conteúdo

O PublicWeb apresenta nome da empresa, slogan, serviços publicados, profissionais, promoções, agendamento, solicitação de demonstração, diferenciais, planos e FAQ quando aplicável ao roteiro.

## Integração

O browser usa `/PublicApi/services`, `/PublicApi/professionals` e `/PublicApi/appointments`, com fallback visual para manter a demonstração mesmo se a API estiver indisponível.

## Configuração pelo Admin

Use `/Admin/PublicSite` para alterar hero, CTAs, SEO, publicação de serviços/profissionais e promoção demo. O preview permite vender o conceito de site configurável por unidade/rede.
