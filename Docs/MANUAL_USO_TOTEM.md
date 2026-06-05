# Manual de Uso — KioskWeb/Totem

## Acesso

- URL local: http://localhost:8083/Kiosk/Services
- Device demo: `KIOSK-DEMO-001`
- Browser deve chamar apenas `/KioskApi`.

## Fluxo obrigatório

1. Escolher serviço.
2. Informar cliente.
3. Escolher profissional.
4. Confirmar dados.
5. Selecionar pagamento mock.
6. Ver sucesso e número da comanda.
7. Enviar avaliação.

## Segurança demonstrativa

O Totem nunca depende de `http://api:8080` no navegador. Se API retornar erro, o proxy KioskApi usa dados demo com HTTP 200.
