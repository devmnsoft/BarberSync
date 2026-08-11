# Demo Wizard - BarberSync Guided Release 19.0

## Objetivo

O **Demo Wizard** (`/Admin/DemoWizard`) é o roteiro guiado da apresentação comercial BarberSync. Ele conduz a pessoa demonstradora por diagnóstico, dashboard, FullServiceFlow, Cliente 360, Agenda, Comanda/PDV, Estoque, Avaliações, PublicWeb e Totem.

## Como executar

1. Suba a stack: `docker compose build --no-cache && docker compose up -d`.
2. Abra `http://localhost:8081/Admin/DemoWizard`.
3. Clique **Iniciar apresentação**.
4. Siga as 15 etapas em sequência usando o botão principal de cada card.

## Etapas demonstráveis

1. Preparar ambiente.
2. Validar diagnóstico.
3. Carregar cenário demo.
4. Abrir Dashboard.
5. Executar FullServiceFlow.
6. Conferir Cliente 360.
7. Conferir Agenda.
8. Conferir Comanda.
9. Conferir Estoque.
10. Conferir Avaliação.
11. Abrir PublicWeb.
12. Realizar agendamento público.
13. Abrir Kiosk.
14. Concluir fluxo do Totem.
15. Finalizar demonstração.

## O que acontece em cada etapa

- O progresso visual muda entre **Pendente**, **Em execução**, **Concluído** e **Atenção**.
- O status é persistido em `localStorage` pela chave `barbersync.demoWizard.progress.v18`.
- Cada ação registra evento no `BarberSyncEventBus`.
- Etapas de cenário, PublicWeb e Kiosk alimentam o `BarberSyncDemoStore` com dados demo locais.
- Os links abrem as telas reais relacionadas sem chamar `http://api:8080` no navegador.

## Como resetar o progresso

- Clique **Resetar progresso** no topo do Demo Wizard.
- Ou limpe manualmente a chave `barbersync.demoWizard.progress.v18` no `localStorage` do Admin.

## Uso junto com FullServiceFlow

1. No Demo Wizard, execute a etapa **Executar FullServiceFlow**.
2. Abra `/Admin/FullServiceFlow`.
3. Clique **Rodar fluxo automático de teste** ou **Validar fluxo completo automaticamente** quando quiser demonstrar execução ponta a ponta.
4. Retorne ao Dashboard para mostrar KPIs/eventos atualizados.
