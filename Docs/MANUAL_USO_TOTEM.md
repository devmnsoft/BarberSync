# Manual de uso — KioskWeb BarberSync 11.0

## Acesso

URL: `http://localhost:8083/Kiosk/Services`.
Device demo: `KIOSK-DEMO-001`.

## Fluxo obrigatório

1. Escolher serviço.
2. Informar telefone e nome do cliente.
3. Escolher profissional.
4. Conferir resumo.
5. Simular pagamento mock.
6. Visualizar sucesso e resumo.
7. Registrar avaliação.

## Resiliência

O Totem consome `/KioskApi/services`, `/KioskApi/professionals`, `/KioskApi/payment/mock` e `/KioskApi/review`. Se a API falhar, o proxy MVC retorna dados demo com status 200 para evitar tela vazia.
