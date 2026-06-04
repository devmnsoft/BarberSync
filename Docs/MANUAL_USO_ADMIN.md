# BarberSync SaaS Platform Demo 8.0 — MANUAL USO ADMIN

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

# Manual do Admin — BarberSync 8.0

## Acesso

Abra `http://localhost:8081/Admin`.

## Menu

- **Operação**: Dashboard, Operação ao Vivo, Lead to Cash, Agenda, Atendimentos, Comandas e Caixa.
- **Cadastros**: Clientes, Profissionais, Serviços e Produtos.
- **Relacionamento**: Jornada do Cliente, Fidelidade, Campanhas, Cupons e Avaliações.
- **Gestão**: SaaS Control Center, Channel Manager, Financeiro, Estoque, Relatórios e Copilot IA.
- **Canais**: Site Público, Totem e Mobile.
- **Sistema**: Demo Experience, Central da Demo, Configurações da Plataforma, Usuários e Perfis, Unidades, Auditoria, Notificações e Manual.

## Configurações

Use **Configurações da Plataforma** para editar empresa, branding, horários, agenda, financeiro e canais. As ações salvam no DemoStore 8.0 e atualizam previews.

## Canais

- **Site Público** publica hero, serviços, profissionais, promoções e SEO.
- **Totem** configura dispositivos, serviços liberados, pagamentos, acessibilidade e logs.
- **Mobile** é demonstrado como canal coerente com serviços, promoções, cashback, notificações, histórico e perfil.

## Governança

Use Usuários e Perfis, Unidades, Auditoria e Notificações para demonstrar controles enterprise sem implementar segurança real nesta demo.
