## Atualização Demo Experience 1.0

O PublicWeb é uma landing comercial com headline, CTA, serviços, profissionais, diferenciais, planos, antes/depois e formulário de agendamento via `/PublicApi/appointments`.

# BarberSync — Manual de Uso PublicWeb

## Objetivo
Vender a proposta BarberSync e permitir agendamento demonstrável para cliente final.

## Seções principais
- Por que escolher o BarberSync.
- Antes e depois.
- Diferenciais.
- Como funciona.
- Totem inteligente.
- App para clientes.
- IA para gestão.
- Planos.
- Solicitar demonstração.

## CTAs
- Agendar serviço.
- Ver painel administrativo.
- Testar totem.
- Solicitar demonstração.

## Agendamento
O formulário envia `POST /PublicApi/appointments` e mostra feedback visual. Se a API estiver indisponível, o fallback demo retorna sucesso controlado.

## Demonstração comercial
Comece pelos diferenciais e planos, clique em “Agendar serviço”, envie uma solicitação e então abra o Admin para mostrar que o mesmo roteiro operacional continua em agenda, cliente 360 e comanda.
