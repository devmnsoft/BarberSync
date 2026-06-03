# BarberSync SaaS Platform Demo 8.0

## Objetivo

A Demo 8.0 apresenta o BarberSync como plataforma SaaS enterprise para barbearias, salões, estética, redes e franquias. O roteiro demonstra gestão centralizada, operação omnichannel, PublicWeb, Totem, Mobile, PDV, caixa, estoque, fidelidade, auditoria, notificações, relatórios e Copilot contextual.

## Arquitetura de canais e proxies

- Admin MVC: `http://localhost:8081/Admin`.
- API Swagger: `http://localhost:8080/swagger`.
- PublicWeb MVC: `http://localhost:8082/`.
- Totem MVC: `http://localhost:8083/Kiosk/Services`.
- O browser usa somente `/AdminApi`, `/PublicApi` e `/KioskApi`.
- A URL interna `http://api:8080` fica restrita ao Docker/server-side por `ApiSettings:BaseUrl` e não deve aparecer em JS, HTML ou CSHTML renderizado ao navegador.

## DemoStore 8.0

O estado comercial da demonstração fica no `localStorage` em `barbersync.demo.state.v8` e centraliza `company`, `branding`, `scheduleRules`, `paymentRules`, `cashbackRules`, `publicWeb`, `kiosk`, `mobile`, `permissions`, `branches` e `notifications`.

Métodos principais: `getSettings`, `updateSettings`, `resetSettings`, `applyBranding`, `publishPublicWebConfig`, `publishKioskConfig` e `publishMobileConfig`.

## Fluxo recomendado

1. Abrir Dashboard 8.0 e explicar filtros por período, unidade e canal.
2. Abrir Configurações da Plataforma e alterar marca, canais e regras.
3. Publicar PublicWeb e mostrar landing comercial.
4. Configurar Totem, simular atendimento, pagamento e avaliação.
5. Entrar em Usuários e Perfis para explicar governança visual.
6. Abrir Unidades para demonstrar franquias/multiunidade.
7. Mostrar Auditoria e Notificações geradas pela operação.
8. Abrir Relatórios 3.0 e exportações mock.
9. Fechar com Copilot 6.0 criando ação contextual.

## Reset

No console do Admin, execute `BarberSyncDemoStore.resetSettings()` para restaurar configurações da plataforma ou limpe o `localStorage` para reset completo do cenário.
