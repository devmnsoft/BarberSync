# Manual de uso do Totem BarberSync

## Fluxo

1. Serviços: carregar `/KioskApi/services?deviceCode=KIOSK-DEMO-001` e escolher um serviço.
2. Cliente: identificar ou cadastrar rapidamente.
3. Profissional: escolher profissional disponível.
4. Confirmação: revisar atendimento.
5. Pagamento: aprovar pagamento mock em `/KioskApi/payment/mock`.
6. Sucesso: exibir protocolo e reiniciar estado.
7. Avaliação: enviar nota por `/KioskApi/review`.

## Regras de demonstração

O Totem salva estado em `sessionStorage`, possui voltar/cancelar, fallback demo e não chama a API Docker diretamente no browser.
