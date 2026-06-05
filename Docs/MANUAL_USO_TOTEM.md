# Manual de uso — Totem/KioskWeb

## Acesso

- URL inicial: `http://localhost:8083/Kiosk/Services`
- Device demo: `KIOSK-DEMO-001`

## Fluxo obrigatório

1. Escolher serviço.
2. Informar cliente.
3. Escolher profissional.
4. Confirmar resumo.
5. Pagar em modo mock.
6. Ver tela de sucesso.
7. Enviar avaliação.

## Resiliência

O browser usa `/KioskApi/...`. Se a API retornar 409, 404, 500, vazio ou ficar indisponível, o proxy mantém serviços e profissionais demo para evitar tela vazia.
