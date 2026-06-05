# KIOSK WEB MVC

Projeto `Web/BarberSync.KioskWeb` em ASP.NET Core MVC para autoatendimento.

- Layout fullscreen: `Views/Shared/_KioskLayout.cshtml`
- Fluxo base: `Kiosk/Index -> Services -> Client -> Confirm -> Success`
- `deviceCode` aceito em `/Kiosk?deviceCode=KIOSK-DEMO-001`
- API configurável por `ApiSettings:BaseUrl` server-side.

## Timeout e reset
Configuração prevista em `appsettings*.json` pela chave `InactivityTimeoutSeconds`.
