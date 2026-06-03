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
