# BarberSync SaaS Platform Demo 8.0 — CHECKLIST DEMO

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

# Checklist Demo BarberSync 8.0

## Pré-demo

- [ ] `dotnet build` concluído.
- [ ] `docker compose build --no-cache` concluído quando aplicável.
- [ ] `docker compose up -d` com API, Admin, PublicWeb, Kiosk, PostgreSQL e Seq ativos.
- [ ] Swagger abre em `http://localhost:8080/swagger`.
- [ ] Admin abre em `http://localhost:8081/Admin`.
- [ ] PublicWeb abre em `http://localhost:8082/`.
- [ ] Totem abre em `http://localhost:8083/Kiosk/Services`.
- [ ] DevTools sem chamada do browser para URL interna da API Docker.

## Proxies

- [ ] `/AdminApi/dashboard` retorna 200.
- [ ] `/AdminApi/clients` retorna 200.
- [ ] `/AdminApi/professionals` retorna 200.
- [ ] `/AdminApi/services` retorna 200.
- [ ] `/AdminApi/appointments` retorna 200.
- [ ] `/AdminApi/service-orders` retorna 200.
- [ ] `/AdminApi/products` retorna 200.
- [ ] `/PublicApi/services` retorna 200.
- [ ] `/PublicApi/professionals` retorna 200.
- [ ] `/KioskApi/services?deviceCode=KIOSK-DEMO-001` retorna 200.

## Roteiro visual

- [ ] Menu final organizado por Operação, Cadastros, Relacionamento, Gestão, Canais e Sistema.
- [ ] Platform Settings salva, restaura e aplica branding.
- [ ] PublicSite publica hero, serviços, profissionais, promoção e SEO demo.
- [ ] Kiosk Manager altera dispositivos, serviços, pagamentos, acessibilidade e logs.
- [ ] Users/Profiles demonstra matriz de permissões.
- [ ] Branches mostra unidades, ranking e comparativo.
- [ ] Audit filtra e exporta eventos mock.
- [ ] Notifications mostra contador na topbar e marca lidas.
- [ ] Reports 3.0 tem filtros, cards, gráfico, tabela e exportações mock.
- [ ] Dashboard 8.0 reage a filtros e colapsa blocos.
- [ ] Copilot 6.0 responde por modo e cria alerta/ação.
