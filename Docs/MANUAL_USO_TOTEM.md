# Manual de Uso — Totem BarberSync

Acesse `http://localhost:8083/Kiosk/Services`.

## Fluxo completo

1. Selecione um serviço.
2. Informe telefone e nome.
3. Escolha o profissional.
4. Revise o resumo.
5. Selecione pagamento mock.
6. Veja a tela de sucesso.
7. Registre avaliação.

O fluxo usa `sessionStorage` para preservar serviço, cliente, profissional, pagamento e avaliação entre páginas. O browser usa somente `/KioskApi`.
