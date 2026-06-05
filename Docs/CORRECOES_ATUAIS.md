# Correções Atuais — BarberSync Quality Gate + Demo Ready 16.0

## Entregas

- Criado `Scripts/quality-gate.ps1` com build, Docker, endpoints, assets, auditoria de URLs proibidas e logs.
- Criada tela `/Admin/Diagnostics` com checks visuais, testes JS do DemoStore, reset e exportação mock.
- Adicionado teste JS `wwwroot/js/tests/demo-store-tests.js` cobrindo DemoStore, EventBus, Dashboard e localStorage.
- Adicionado botão de validação automática no FullServiceFlow.
- Adicionado painel `Status da Demonstração` no Dashboard.
- Adicionados endpoints demo de produtos e comandas na API para reduzir fallback por 404.
- Adicionados smoke tests .NET para API e proxies MVC.

## Auditoria de comunicação

A comunicação browser-side continua restrita a proxies MVC e rotas relativas. A URL interna Docker da API permanece apenas em configuração Docker/server-side e documentação operacional.

## Pendências conhecidas

A validação visual completa depende de ambiente com Docker e portas 8080–8083 livres.
