# BarberSync SaaS Platform Demo 10.0 — Correções atuais

Data da consolidação: 2026-06-04.

## Diagnóstico encontrado

- O ambiente de execução do agente não possui `dotnet` nem `docker`, então a validação de build/containers ficou registrada como limitação de ambiente e deve ser repetida em uma estação com SDK .NET 8 e Docker Engine.
- O `docker-compose.yml` dependia de `env_file: .env`; quando o arquivo não existia, a subida do stack ficava frágil para demonstração limpa.
- A API `/api/services` retornava lista vazia, fazendo AdminWeb/PublicWeb dependerem de fallback visual mesmo quando a API respondia 200.
- Os proxies MVC não tratavam payloads 200 semanticamente vazios como fallback demo, o que poderia deixar tabelas/cards sem dados.
- O PublicWeb tinha fallback com menos serviços/profissionais que o catálogo obrigatório da demo 10.0.
- O fluxo Kiosk salvava `selectedService`, `selectedProfessional` e `selectedPayment` de forma mínima; foi consolidado para objetos JSON em `sessionStorage`.

## Correções realizadas

- Docker Compose consolidado sem dependência obrigatória de `.env`, com defaults seguros para PostgreSQL, healthcheck, `ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT=Docker`, BaseUrl server-side para MVC e Seq configurado.
- API `/api/services` passou a entregar catálogo demo real com Corte Masculino, Barba Tradicional, Corte + Barba, Sobrancelha, Hidratação Capilar e Manicure.
- API Kiosk recebeu endpoints demo complementares para status, cliente, pagamento mock e avaliação, sempre com resposta 200 em modo demonstração.
- `ConfigurationService` passou a expor catálogo público/totem completo e profissionais obrigatórios da demo.
- AdminApi e PublicApi passaram a detectar payloads vazios (`items: []`, `data.items: []`, arrays vazios) e retornar fallback demo com 200.
- PublicApi foi marcado como `ApiController` e ganhou fallback completo para serviços e profissionais.
- KioskFlow passou a persistir `selectedService`, `selectedClient`, `selectedProfessional` e `selectedPayment` no `sessionStorage` em formato JSON.

## Validações de proxy

O browser deve chamar somente:

- `/AdminApi/...`
- `/PublicApi/...`
- `/KioskApi/...`

`http://api:8080` permanece restrito a Docker/server-side no `docker-compose.yml` e appsettings Docker.

## Pendências reais

- Reexecutar `dotnet build`, builds por projeto e `docker compose build --no-cache` em ambiente com SDK .NET/Docker instalados.
- Reexecutar validação HTTP/PowerShell após containers subirem.
- Fazer validação visual em navegador/DevTools para confirmar ausência de chamadas browser-side a `http://api:8080` e ausência de erros críticos de console.
