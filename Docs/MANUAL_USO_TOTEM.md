## Atualização Demo Experience 1.0

O Totem usa `/KioskApi/...`, salva o fluxo em sessionStorage, permite voltar/cancelar, exibe resumo lateral, possui tela `/Kiosk/Help`, simula pagamento e reseta a tela de sucesso após 10 segundos.

# BarberSync — Manual de Uso do Totem

## Fluxo guiado
1. Abrir `/Kiosk/Services`.
2. Selecionar serviço.
3. Informar telefone e nome.
4. Selecionar profissional.
5. Confirmar resumo lateral.
6. Selecionar pagamento mock.
7. Visualizar sucesso.
8. Registrar avaliação com estrelas grandes.

## Recursos de demonstração
- Barra de progresso por etapa.
- Botão “Preciso de ajuda”.
- Botão de acessibilidade/alto contraste.
- Resumo lateral com serviço, cliente, profissional e pagamento.
- Timeout visual animado.
- Persistência em `sessionStorage`: `selectedService`, `selectedClient`, `selectedProfessional` e `selectedPayment`.

## Integração segura
O navegador chama somente `/KioskApi/...`; a URL interna da API fica no servidor MVC.

## Roteiro rápido para vendas
Mostre o fluxo em voz alta: serviço escolhido, cliente identificado, profissional selecionado, pagamento mock aprovado e avaliação final. O resumo lateral deve ser usado para confirmar que o estado foi preservado entre telas.
