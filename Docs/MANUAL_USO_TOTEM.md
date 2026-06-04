# Manual de uso — Totem/Kiosk BarberSync Demo 10.0

## Acesso

URL: `http://localhost:8083/Kiosk/Services`.

## Fluxo

1. Escolha um serviço.
2. Informe telefone/nome do cliente.
3. Escolha profissional.
4. Confirme dados.
5. Simule pagamento em PIX, cartão ou recepção.
6. Veja sucesso/comanda.
7. Registre avaliação.

## Persistência local

O fluxo grava em `sessionStorage`:

- `selectedService`
- `selectedClient`
- `selectedProfessional`
- `selectedPayment`
- `kiosk-flow`
- `kiosk-summary`

## Proxy

O browser chama somente `/KioskApi/...`; a comunicação com a API real é server-side.
