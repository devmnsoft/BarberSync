# BarberSync SaaS Platform Demo 7.0

## Objetivo
Transformar o BarberSync em uma demonstração SaaS enterprise vendável para barbearias, salões, estética e redes franqueadas.

## Arquitetura dos proxies
- O navegador do Admin usa somente `/AdminApi/...`.
- O navegador do PublicWeb usa somente `/PublicApi/...`.
- O navegador do Totem usa somente `/KioskApi/...`.
- `ApiSettings:BaseUrl` e `ApiBaseUrl` permanecem server-side para Docker/local.
- No Docker, os serviços MVC acessam a API por `http://api:8080`; isso não deve ser renderizado em JS/CSHTML do browser.

## Módulos 7.0
- Configurações da Plataforma: empresa, identidade, horários, regras, financeiro e canais.
- Site Público: publicação, hero, serviços, profissionais, promoções e SEO.
- Totem: dispositivos, serviços liberados, pagamentos, acessibilidade, experiência e logs.
- Usuários e Perfis: perfis, unidade e permissões visuais.
- Multiunidade: comparativo entre unidades/franquias.
- Auditoria: EventBus, ações, cadastros, agenda, PDV, estoque, Copilot e Totem.
- Notificações: alertas internos e preferências demo.
- Dashboard 7.0, Reports 2.0 e Copilot 5.0.

## Como resetar e carregar cenários
Use a Central da Demo ou o botão de reset no Admin para recarregar o DemoStore/localStorage. Em uma apresentação, comece em `/Admin/DemoCenter`, carregue o cenário e siga para Dashboard, PublicWeb, Totem e Lead to Cash.
