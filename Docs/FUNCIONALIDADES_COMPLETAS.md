# BarberSync SaaS Platform Demo 8.0 — FUNCIONALIDADES COMPLETAS

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

# Funcionalidades Completas — BarberSync SaaS Platform Demo 8.0

## Plataforma

- Dashboard 8.0 omnichannel.
- Platform Settings 2.0.
- Public Site Manager 2.0.
- Kiosk Manager 2.0.
- Users and Permissions 2.0.
- Branches/Franchise 2.0.
- Audit Center 2.0.
- Notification Center 2.0.
- Reports 3.0.
- Copilot 6.0.

## Operação

- Agenda, atendimentos, comandas, caixa, pagamentos e recibos mock.
- Estoque com alerta crítico e movimentações.
- Campanhas, cupons, cashback, fidelidade e avaliações.
- Lead to Cash com origem PublicWeb/Totem/Admin.

## Canais

- Admin MVC enterprise.
- PublicWeb vendedor e configurável.
- Totem com autoatendimento, acessibilidade, pagamento mock, ajuda e avaliação.
- Mobile React Native visualmente coerente.

## Governança

- Perfis e permissões visuais.
- Auditoria e EventBus demo.
- Notificações internas com contador.
- Multiunidade/franquia com comparativo executivo.
