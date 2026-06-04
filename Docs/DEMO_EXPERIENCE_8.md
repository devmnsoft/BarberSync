# BarberSync SaaS Platform Demo 8.0 — DEMO EXPERIENCE 8

> Documento atualizado para a etapa 8.0 da demonstração comercial BarberSync.

## Escopo 8.0

- Plataforma SaaS demonstrável com Admin, PublicWeb, Totem, Mobile, API, PostgreSQL, Docker Compose e Seq.
- Configurações centralizadas no DemoStore/localStorage `barbersync.demo.state.v8`.
- Proxies obrigatórios para browser: `/AdminApi`, `/PublicApi` e `/KioskApi`.
- `ApiSettings:BaseUrl` e `http://api:8080` ficam restritos ao server-side/Docker.

## Como rodar Docker

```bash
docker compose build --no-cache
docker compose up -d
docker compose ps
```

URLs de demonstração: API `http://localhost:8080/swagger`, Admin `http://localhost:8081/Admin`, PublicWeb `http://localhost:8082/`, Totem `http://localhost:8083/Kiosk/Services`.

## Como rodar Windows/local

1. Configure a API local em `http://localhost:8080`.
2. Configure Admin/Public/Kiosk para usar os proxies server-side.
3. Abra as URLs principais e valide assets CSS/JS.

## Reset e cenários da demo

- No Admin, use Configurações da Plataforma para restaurar padrão.
- Para reset manual, remova `barbersync.demo.state.v8` do localStorage.
- Cenários comerciais: multiunidade, operação ao vivo, lead-to-cash, estoque crítico, campanha/Copilot e Totem com pagamento mock.

## Demonstrações obrigatórias

- Configurações: empresa, branding, horários, agenda, financeiro e canais.
- Multiunidade: ranking, comparativo, receita, agendamentos, ticket, avaliação, ocupação e estoque crítico.
- Canais: PublicWeb publicado, Totem liberado/configurado e Mobile visual coerente.
- Auditoria: eventos do EventBus, ações de usuário, status, comandas, estoque, campanhas e Copilot.
- Notificações: contador no topbar, lista, marcar como lida e abrir módulo relacionado.
- Relatórios: filtros, cards, gráficos, tabela, imprimir, exportar PDF/Excel mock e favorito.

## Checklist comercial rápido

- Platform Settings 2.0 salva no DemoStore e aplica branding.
- Public Site Manager 2.0 publica hero, serviços, profissionais, promoções e SEO demo.
- Kiosk Manager 2.0 gerencia dispositivos, serviços, pagamentos, acessibilidade e logs.
- Users/Profiles mostra matriz visual de permissões.
- Branches/Franchise mostra quatro unidades demo.
- Audit e Notifications estão preenchidos e acionáveis.
- Reports 3.0, Dashboard 8.0 e Copilot 6.0 têm ações reais de interface.


## Conteúdo anterior

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
